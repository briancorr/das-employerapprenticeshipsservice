#Requires -RunAsAdministrator

$certpwd = ConvertTo-SecureString -String password -Force -AsPlainText

Import-PfxCertificate -FilePath localhost.pfx -CertStoreLocation cert://LocalMachine/My -Password $certpwd -Exportable
Import-PfxCertificate -FilePath localhost.pfx -CertStoreLocation cert://LocalMachine/Root -Password $certpwd -Exportable
Import-PfxCertificate -FilePath DasAmlCert.pfx -CertStoreLocation cert://LocalMachine/My -Password $certpwd -Exportable
Import-PfxCertificate -FilePath DasAmlCert.pfx -CertStoreLocation cert://LocalMachine/Root -Password $certpwd -Exportable