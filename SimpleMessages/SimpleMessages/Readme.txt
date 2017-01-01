*** MAKE SS ROOT CA ***
makecert.exe -sk RootCA -sky signature -pe -n CN=Kanturna -r -sr LocalMachine -ss Root SimpleMessagesCA.cer

*** MAKE PERSONAL CERT ***
makecert.exe -sk server -sky exchange -pe -n CN=Kanturna -ir LocalMachine -is Root -ic SimpleMessagesCA.cer -sr LocalMachine -ss My SimpleMessagesTest.cer

*** GET PERSONAL CERT THUMBPRINT ***
certutil -store My

*** ADD/REMOVE PORT RESERVATION ***
netsh http add    urlacl url=https://+:14222/ user=EVERYONE
netsh http delete urlacl url=https://+:14222/

*** ADD/REMOVE SSL SERVER CERT BINDING ***
netsh http add    sslcert ipport=0.0.0.0:14222 appid={2b092e34-37fa-4cf1-8321-7a1363a3a8f2} certhash=8009bd7e362b11ede6c0d9ef16f3d7bcd8dbdd14
netsh http delete sslcert ipport=0.0.0.0:14222