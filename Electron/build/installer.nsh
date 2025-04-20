Section "CustomInstall"
  SetOutPath "$PROGRAMFILES\Saad Khawaja\Instant Clipboard"
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\Saad Khawaja\Instant Clipboard"
  CreateShortCut "$SMPROGRAMS\Saad Khawaja\Instant Clipboard\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
SectionEnd