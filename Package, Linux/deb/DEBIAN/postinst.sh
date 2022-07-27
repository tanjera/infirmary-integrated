#!/bin/bash
sudo xdg-mime install /usr/share/mime/packages/infirmary-integrated.xml
sudo xdg-icon-resource install --context mimetypes --size 1024 /usr/share/pixmaps/infirmary-integrated.png application-infirmary-integrated
sudo update-mime-database /usr/share/mime
sudo xdg-mime default infirmary-integrated.desktop application/infirmary-integrated
sudo update-desktop-database