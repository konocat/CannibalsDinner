using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CannibalsDinner
{
    public partial class MainWindow : Window
    {
        private int counter;
        private int energy;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private DispatcherTimer energyTimer;
        private DispatcherTimer idleTimer;
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            counter = 0;
            energy = 100;
            Counter.Content = counter.ToString();
            CounterEnergy.Content = energy.ToString();

            // Настройка таймера для восполнения энергии
            energyTimer = new DispatcherTimer();
            energyTimer.Interval = TimeSpan.FromMilliseconds(100); // Каждые 100 миллисекунд
            energyTimer.Tick += EnergyTimer_Tick;

            // Настройка таймера для отслеживания бездействия
            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromSeconds(2); // 2 секунды
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();

            // Инициализация CancellationTokenSource
            cancellationTokenSource = new CancellationTokenSource();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ProcessClick(), cancellationTokenSource.Token);
        }

        private void ProcessClick()
        {
            Console.WriteLine($"ProcessClick started on thread: {Thread.CurrentThread.ManagedThreadId}");
            semaphore.Wait();
            try
            {
                if (energy > 0)
                {
                    counter++;
                    energy--;
                }

                Dispatcher.Invoke(() =>
                {
                    Counter.Content = counter.ToString();
                    CounterEnergy.Content = energy.ToString();
                });

                // Сброс таймера бездействия
                idleTimer.Stop();
                idleTimer.Start();

                // Остановка таймера восполнения энергии, если он запущен
                if (energyTimer.IsEnabled)
                {
                    energyTimer.Stop();
                }
            }
            finally
            {
                semaphore.Release();
            }
            Console.WriteLine($"ProcessClick finished on thread: {Thread.CurrentThread.ManagedThreadId}");
        }

        private void EnergyTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine($"EnergyTimer_Tick started on thread: {Thread.CurrentThread.ManagedThreadId}");
            semaphore.Wait();
            try
            {
                if (energy < 100)
                {
                    energy++;
                    Dispatcher.Invoke(() =>
                    {
                        CounterEnergy.Content = energy.ToString();
                    });
                }
                else
                {
                    energyTimer.Stop();
                    Dispatcher.Invoke(() =>
                    {
                        Chef.Visibility = Visibility.Hidden;
                        Chef_sleep.Visibility = Visibility.Visible;
                        PotButton.Source = new BitmapImage(new Uri("/assets/images/pot_active.png", UriKind.Relative));
                    });
                }
            }
            finally
            {
                semaphore.Release();
            }
            Console.WriteLine($"EnergyTimer_Tick finished on thread: {Thread.CurrentThread.ManagedThreadId}");
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            // Запуск таймера восполнения энергии, если энергия меньше или равна 25
            if (energy <= 25 && !energyTimer.IsEnabled)
            {
                energyTimer.Start();
                Dispatcher.Invoke(() =>
                {
                    Chef.Visibility = Visibility.Visible;
                    Chef_sleep.Visibility = Visibility.Hidden;
                    PotButton.Source = new BitmapImage(new Uri("/assets/images/pot_ready.png", UriKind.Relative));
                });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            energyTimer.Stop();
            idleTimer.Stop();
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            semaphore.Dispose();
        }
    }
}