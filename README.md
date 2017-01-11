# simple-messages
Simple client-server chat based on ASP.NET MVC (GUI) and WCF

# Development environment setup

*Visual studio 2015*

Run VS under administrator priviledges, otherwise host will fail to open (registration/reservatio needed for port).
Open solution and set multiple startup projects - select projects .Web and .Svc.
Web client is based on ASP.NET MVC framework.
Service is a simple self-hosted WCF REST service.

*NOTE: perform package restore; it should be restored during rebuild, otherwise check VS settings for package restore*

*SQL Server 2014*

Perform database restore to your SQL Srver 2014
After database restore, create/fix user using...
    ``EXEC sp_change_users_login 'Auto_Fix', 'MessagesDbLocalUser'``
Setup connection string in web.config and app.config of web application and service appropriately.

*Other notes*
 * to test, register at least two users (multi-user chat supported, no conference/group mode)
 * either open in different browsers or send messages and logout/login with each 