Development environment

* Visual studio 2015
  - run VS under administrator priviledges, otherwise host will fail to open (registration/reservatio needed for port)
  - solution - set multiple startup projects (.Web and .Svc)
    - web is MVC
	- Svc is self-hosted WCF REST service
  - restore packages if not restored (should be restored during rebuild, otherwise check VS settings for package restore)

* SQL Server 2014
  - perform database restore
  - after database restore, create/fix user
    EXEC sp_change_users_login 'Auto_Fix', 'MessagesDbLocalUser'
  - setup connection string in web.config appropriatelly

Notes

 * to test, register at least two users (multi-user chat supported, no conference/group mode)
 * either open in different browsers or send messages and logout/login with each 

