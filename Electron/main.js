require('dotenv').config();

const { app, Tray, Menu, clipboard, nativeImage, dialog } = require('electron');
const path = require('path');
const fs = require('fs');

let tray = null;
let lastTextContent = '';
let lastImageContent = null;

// Ensure only one instance of the app is running
const gotTheLock = app.requestSingleInstanceLock();

if (!gotTheLock) {
  app.quit();
} else {
  app.on('second-instance', (event, commandLine, workingDirectory) => {
    // When another instance is launched, focus the main window if you have one
    dialog.showMessageBox({
      type: 'warning',
      title: 'Instant Clipboard',
      message: 'Instant Clipboard is already running.',
      buttons: ['OK']
    });
  });

  app.on('ready', () => {
    // Change the app name
    app.setName('Instant Clipboard');

    // Create the tray icon
    const iconPath = path.join(__dirname, 'icon.ico'); // Ensure you have an icon.ico file in the directory
    tray = new Tray(iconPath);

    const contextMenu = Menu.buildFromTemplate([
      {
        label: 'About',
        click: showAboutDialog
      },
      {
        label: 'Clear Clipboard',
        click: clearClipboardFolder
      },
      { type: 'separator' },
      {
        label: 'Quit',
        click: () => { app.quit(); }
      }
    ]);

    tray.setToolTip('Instant Clipboard');
    tray.setContextMenu(contextMenu);

    // Check clipboard periodically (every 1 second in this example)
    setInterval(checkClipboard, 1000);
  });

  function showAboutDialog() {
    dialog.showMessageBox({
      type: 'info',
      title: 'About Instant Clipboard',
      message: `Instant Clipboard\nVersion ${app.getVersion()}\n\nAn Electron app for managing clipboard content.`,
      buttons: ['OK']
    });
  }

  function checkClipboard() {
    try {
      const textContent = clipboard.readText();
      const imageContent = clipboard.readImage();

      if (textContent && textContent !== lastTextContent) {
        handleClipboardContent(textContent, 'text/plain');
        lastTextContent = textContent;
      } else if (!imageContent.isEmpty() && !imageContent.toDataURL().includes(lastImageContent)) {
        handleClipboardContent(imageContent, 'image/png');
        lastImageContent = imageContent.toDataURL();
      }
    } catch (error) {
      console.error("Error reading clipboard:", error);
    }
  }

  function handleClipboardContent(content, mimeType) {
    if (mimeType.startsWith('image/')) {
      saveImage(content);
    } else if (mimeType === 'text/plain') {
      saveText(content);
    } else {
      console.log('Unsupported clipboard content type:', mimeType);
    }
  }

  function saveImage(imageData) {
    const documentsPath = app.getPath('documents');
    const instantClipboardPath = path.join(documentsPath, 'instant-clipboard');

    // Ensure the directory exists
    fs.mkdirSync(instantClipboardPath, { recursive: true });

    const imagePath = path.join(instantClipboardPath, `image.png`);

    fs.writeFile(imagePath, imageData.toPNG(), (err) => {
      if (err) {
        console.error('Error saving image:', err);
      } else {
        console.log('Image saved successfully:', imagePath);
        updateLastAction('image');
      }
    });
  }

  function saveText(textContent) {
    const documentsPath = app.getPath('documents');
    const instantClipboardPath = path.join(documentsPath, 'instant-clipboard');

    // Ensure the directory exists
    fs.mkdirSync(instantClipboardPath, { recursive: true });

    const textPath = path.join(instantClipboardPath, 'text.txt');

    fs.writeFile(textPath, textContent, (err) => {
      if (err) {
        console.error('Error saving text:', err);
      } else {
        console.log('Text saved successfully:', textPath);
        updateLastAction('text');
      }
    });
  }

  function updateLastAction(action) {
    const documentsPath = app.getPath('documents');
    const instantClipboardPath = path.join(documentsPath, 'instant-clipboard');
    
    const jsonPath = path.join(instantClipboardPath, 'clipboard.json');
    const jsonContent = JSON.stringify({ lastAction: action, version: app.getVersion() });

    fs.writeFile(jsonPath, jsonContent, (err) => {
      if (err) {
        console.error('Error updating clipboard.json:', err);
      } else {
        console.log('clipboard.json updated successfully:', jsonPath);
      }
    });
  }

  function clearClipboardFolder() {
    const documentsPath = app.getPath('documents');
    const instantClipboardPath = path.join(documentsPath, 'instant-clipboard');

    fs.readdir(instantClipboardPath, (err, files) => {
      if (err) {
        console.error('Error reading directory:', err);
        return;
      }

      for (const file of files) {
        fs.unlink(path.join(instantClipboardPath, file), err => {
          if (err) {
            console.error('Error deleting file:', err);
          }
        });
      }
      console.log('Clipboard folder cleared.');
      updateLastAction('clear');
    });
  }

  app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
      app.quit();
    }
  });

  app.on('activate', () => {});
}
