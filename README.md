# CannibalsDinner
Приложение написано с использованием WPF. Цель разработки приложение - реализация многопоточности.
Во время реализации многопоточность использовались семафоры.

При нажатии на котел, расходуется энергия и обновляется счетчик слева.

![image](https://github.com/user-attachments/assets/1bcd042f-668b-40be-9ed3-2d5c88bb8503)

Если энергия пользователя больше или равна 25, через 2 секунды после последнего нажатия на котел просыпается поток с поваром и восполняет энергию.
При повторном нажатии во время восстановления энергии, поток останавливается, и необходимо опять подождать 2 секунды.
При достижении 100 единиц энергии, поток с поваром так же останавливается.
За обновление счетчиков отвечает другой поток.

![image](https://github.com/user-attachments/assets/64997c24-7920-4ab4-9135-8e08d946344c)

Для удобства тестирования многопоточности старт и конец выполнения задачи в потоке выводится в консоль.

![Uploading image.png…]()
