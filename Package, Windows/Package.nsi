; includes
!include "FileAssociation.nsh"

; definitions
!define NAME "Infirmary Integrated"


; Named variables
Name ${NAME}
OutFile "infirmary-integrated-.exe"

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
  File /r "publish\"
  
  ; Create application shortcut (first in installation dir to have the correct "start in" target)  
  CreateShortCut "$INSTDIR\${Name}.lnk" "$INSTDIR\${NAME}.exe"

  ; Start menu entries
  SetOutPath "$SMPROGRAMS\${Name}\"
  CopyFiles "$INSTDIR\${Name}.lnk" "$SMPROGRAMS\${NAME}\"
  Delete "$INSTDIR\${Name}.lnk"

  ; Register file association
  ${registerExtension} "$INSTDIR\${NAME}.exe" ".ii" "Infirmary_Integrated_Scenario"

SectionEnd
