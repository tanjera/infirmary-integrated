; includes
!include "FileAssociation.nsh"

; definitions
!define NAME "Infirmary Integrated"
!define NAME_SIMULATOR "Infirmary Integrated"
!define NAME_SCENARIOEDITOR "Infirmary Integrated Scenario Editor"


; Named variables
Name ${NAME}
OutFile "infirmary-integrated-win.exe"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

; Build Unicode installer
Unicode True

; The default installation directory
InstallDir "$PROGRAMFILES\Infirmary Integrated"

;--------------------------------

; Pages

Page directory
Page instfiles

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File /r /x "Package.nsi" ".\*"

  ; Create the uninstaller and associated registry keys
  WriteUninstaller "$INSTDIR\uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Infirmary Integrated" \
                 "DisplayName" "Infirmary Integrated" ; <-- Package_Windows.cs EDIT <--
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Infirmary Integrated" \
                 "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Infirmary Integrated" \
                 "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Infirmary Integrated" \
                 "DisplayIcon" "$\"$INSTDIR\${NAME_SIMULATOR}\Icon_II.ico$\""
             
  ; Create application shortcut (first in installation dir to have the correct "start in" target)  
  CreateShortCut "$INSTDIR\${NAME_SIMULATOR}.lnk" "$INSTDIR\${NAME_SIMULATOR}\${NAME_SIMULATOR}.exe"
  CreateShortCut "$INSTDIR\${NAME_SCENARIOEDITOR}.lnk" "$INSTDIR\${NAME_SCENARIOEDITOR}\${NAME_SCENARIOEDITOR}.exe"

  ; Start menu entries
  SetOutPath "$SMPROGRAMS\${Name}\"
  CopyFiles "$INSTDIR\${NAME_SIMULATOR}.lnk" "$SMPROGRAMS\${NAME}\"
  CopyFiles "$INSTDIR\${NAME_SCENARIOEDITOR}.lnk" "$SMPROGRAMS\${NAME}\"
  Delete "$INSTDIR\${NAME_SIMULATOR}.lnk"
  Delete "$INSTDIR\${NAME_SCENARIOEDITOR}.lnk"

  ; Register file association
  ${registerExtension} "$INSTDIR\${NAME_SIMULATOR}\${NAME_SIMULATOR}.exe" ".ii" "Infirmary Integrated Scenario"

  ; Register icon for .ii files
  WriteRegStr HKCR ".ii" "" "iiFile"
  WriteRegStr HKCR "iiFile\DefaultIcon\" "" "$\"$INSTDIR\${NAME_SIMULATOR}\Icon_II.ico$\""

SectionEnd

;--------------------------------

Section "Uninstall"
  ${unregisterExtension} ".ii" "Infirmary Integrated Scenario"
  
  RMDir /r $INSTDIR
  RMDir /r "$SMPROGRAMS\${Name}"
  
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Infirmary Integrated"
  DeleteRegKey HKCR ".ii"
  DeleteRegKey HKCR "iiFile"
SectionEnd