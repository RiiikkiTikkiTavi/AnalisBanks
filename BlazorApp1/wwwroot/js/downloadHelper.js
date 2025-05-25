window.downloadFileFromBytes = (filename, bytesBase64) => {
    // Создаём элемент <a> = ссылка
    const link = document.createElement('a');

    // Устанавливаем имя файла, которое предложит браузер при скачивании
    link.download = filename;

    // Устанавливаем содержимое файла в формате base64 
    link.href = "data:application/octet-stream;base64," + bytesBase64;

    // Добавляем ссылку во временный DOM
    document.body.appendChild(link);

    // Имитируем клик по ссылке, чтобы запустить скачивание
    link.click();

    // Удаляем временно добавленную ссылку
    document.body.removeChild(link);
};