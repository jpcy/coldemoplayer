;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "compLexity Demo Player 1.1.9"
  OutFile "coldemoplayer119_install.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\compLexity Demo Player"

  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\compLexity Demo Player" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

  SetCompressor /SOLID lzma
  
  InstProgressFlags smooth colored
  
  Var runProgram
  
;--------------------------------
;Interface Settings

  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "win.bmp"
  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Icon

  Icon "..\compLexity Demo Player\data\col.ico"

;--------------------------------

Function .onInstSuccess

  ; associate with .dem files
  WriteRegStr HKCR ".dem" "" "compLexity Demo Player"
  WriteRegStr HKCR "compLexity Demo Player\DefaultIcon" "" "$INSTDIR\compLexity Demo Player.exe"
  WriteRegStr HKCR "compLexity Demo Player\Shell\open" "" "Open with compLexity Demo Player"
  WriteRegStr HKCR "compLexity Demo Player\Shell\open\command" "" '"$INSTDIR\compLexity Demo Player.exe" "%1"'
  
  ${If} $runProgram == 1
    Exec "$INSTDIR\compLexity Demo Player.exe"
  ${EndIf}
  
FunctionEnd

;--------------------------------
;Installer Sections

Section ""

  SectionIn RO

  ;*** BEGIN INSTALL FILES ***
  CreateDirectory "$LOCALAPPDATA\compLexity Demo Player"
  
  ; root
  SetOutPath "$INSTDIR"
  File "..\bin\compLexity Demo Player.exe"
  File "..\bin\compLexity Demo Player.exe.config"
  File "..\bin\HtmlAgilityPack.dll"
  File "..\bin\ICSharpCode.SharpZipLib.dll"
  File "..\bin\Interop.Shell32.dll"
  File "..\bin\ZedGraph.dll"
  File "..\bin\readme.txt"
  File "..\bin\copying.txt"
  
  ; dotnet framework client profile installer
  File "DotNetFx35ClientSetup.exe"
  
  ; config
  CreateDirectory "$INSTDIR\config"
  SetOutPath "$INSTDIR\config"
  File "..\bin\config\steam.xml"
  
  ; game configs
  CreateDirectory "$INSTDIR\config\goldsrc"
  SetOutPath "$INSTDIR\config\goldsrc"
  File /r "..\bin\config\goldsrc\*.xml"
  
  ; icons
  CreateDirectory "$INSTDIR\icons"
  SetOutPath "$INSTDIR\icons"
  File /r "..\bin\icons\*.ico"

  ; maps
  ;CreateDirectory "$INSTDIR\maps"
  ;SetOutPath "$INSTDIR\maps"
  ;File /r "..\bin\maps\*.bsp" "..\bin\maps\*.wad"
  
  ; overviews
  CreateDirectory "$INSTDIR\overviews"
  SetOutPath "$INSTDIR\overviews"
  File /r "..\bin\overviews\*.jpg"
  
  ; previews
  CreateDirectory "$INSTDIR\previews"
  SetOutPath "$INSTDIR\previews"
  File /r "..\bin\previews\*.jpg"
  
  ;*** END INSTALL FILES ***
  
  ; start menu shortcuts
  CreateDirectory "$SMPROGRAMS\compLexity Demo Player"
  CreateShortCut "$SMPROGRAMS\compLexity Demo Player\compLexity Demo Player.lnk" "$INSTDIR\compLexity Demo Player.exe"
  CreateShortCut "$SMPROGRAMS\compLexity Demo Player\View Readme.lnk" "$INSTDIR\readme.txt"
  CreateShortCut "$SMPROGRAMS\compLexity Demo Player\View Licence.lnk" "$INSTDIR\copying.txt"
  CreateShortCut "$SMPROGRAMS\compLexity Demo Player\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  
  ; desktop shortcut
  CreateShortCut "$DESKTOP\compLexity Demo Player.lnk" "$INSTDIR\compLexity Demo Player.exe"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\compLexity Demo Player" "" $INSTDIR

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
SectionEnd

;--------------------------------

Section -PostRequisites

  Var /GLOBAL windowsVersion
  
  ; check for NT
  ReadRegStr $R0 HKLM \
  "SOFTWARE\Microsoft\Windows NT\CurrentVersion" CurrentVersion
  IfErrors installdotnet 0
  StrCpy $windowsVersion $R0 1
  StrCmp $windowsVersion '6' vista xp 
  
  vista:
  StrCpy $runProgram 1
  Goto done
  
  xp:
  StrCpy $0 0
  ; Check for .NET framework install
  loop:
  EnumRegKey $1 HKLM "Software\Microsoft\NET Framework Setup\NDP" $0
  IfErrors installdotnet 0
  StrCmp $1 "" installdotnet 0 ; Empty string means no more keys
  StrCpy $2 $1 1 1 ; Get the 2nd character (format is v2.0.50727, v3.0, v3.5 etc.)
  ${If} $2 >= 3 ; If the .NET framework version is 3 or higher, skip the install prompt
    StrCpy $runProgram 1
    Goto done
  ${EndIf}
  IntOp $0 $0 + 1
  Goto loop

  installdotnet:
  StrCpy $runProgram 0
  
  MessageBox MB_YESNO "The Microsoft .NET Framework 3.0 runtime is required. Download and install the newest version of the framework now?" /SD IDYES IDNO done
  ExecWait '"$INSTDIR\DotNetFx35ClientSetup.exe"' $0
  
  ${If} $0 = 0 ; success
    StrCpy $runProgram 1
  ${EndIf}
  
  done:
  
SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ; install files
  Delete "$INSTDIR\compLexity Demo Player.exe"
  Delete "$INSTDIR\compLexity Demo Player.exe.config"
  Delete "$INSTDIR\HtmlAgilityPack.dll"
  Delete "$INSTDIR\ICSharpCode.SharpZipLib.dll"
  Delete "$INSTDIR\Interop.Shell32.dll"
  Delete "$INSTDIR\ZedGraph.dll"
  Delete "$INSTDIR\readme.txt"
  Delete "$INSTDIR\copying.txt"
  Delete "$INSTDIR\Uninstall.exe"
  
  RMDir /r "$INSTDIR\config"
  RMDir /r "$INSTDIR\icons"
  ;RMDir /r "$INSTDIR\maps"
  RMDir /r "$INSTDIR\overviews"
  RMDir /r "$INSTDIR\previews"
  
  Delete "$INSTDIR\DotNetFx35ClientSetup.exe"

  RMDir "$INSTDIR"

  ; shortcuts
  Delete "$SMPROGRAMS\compLexity Demo Player\compLexity Demo Player.lnk"
  Delete "$SMPROGRAMS\compLexity Demo Player\View Readme.lnk"
  Delete "$SMPROGRAMS\compLexity Demo Player\Uninstall.lnk"
  RMDir "$SMPROGRAMS\compLexity Demo Player"
  Delete "$DESKTOP\compLexity Demo Player.lnk"
  
  ; registry
  DeleteRegKey /ifempty HKCU "Software\compLexity Demo Player"
  
  ; remove file association
  DeleteRegKey HKCR "compLexity Demo Player"
  ReadRegStr $0 HKCR ".dem" ""
  
  StrCmp $0 "compLexity Demo Player" remove 0
  Return
  
  remove:
  DeleteRegKey HKCR ".dem"
SectionEnd
