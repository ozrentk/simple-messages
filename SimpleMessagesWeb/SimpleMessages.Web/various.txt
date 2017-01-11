Automapper
 * installed v4.2.1, because the newer version introduces problems in vs2013 environment
 * installed into website and database projects to be able to map business model and viewmodel

Log4net
 * installed v1.2.10 to both projects to be able to log everything everywhere
 * startup/config in AssemblyInfo.cs
 * Added AdoNetAppender2, programatically sets connection string
   * connection string is in connectionStrings.config
 * configuration file separate (log4net.config)
   * rolling file appender with a nice name and 30 backups
   * easily configurable AdoNetAppender
 * in web.config added trace diagnostics (log4net.txt file)
 * Global.asax is logged

EntityFramework
 * installed v6.1.3 to both projects (required)
 * connection string is in connectionStrings.config
 * database-first designer: DB.edmx

requireJS
 * installed last version as of 2016-11-03 (js module loader)

jQuery
 * installed last version as of 2016-11-03
 * added jQuery.Validation (validation plugin, validates form data)
 * added Microsoft.jQuery.Unobtrusive.Validation (adds validation via data- attributes, like validation-summary-valid)

bootstrap
 * installed last version as of 2016-11-03
 * added Bootstrap.Switch

bundling and minification
 * installed last version as of 2016-11-03
 * requires some dependencies
   * Antlr
   * Newtonsoft.Json
   * WebGrease

respond
 * polyfill that enables older browsers (old IE) to use CSS3 @media screen

Helpers
 * RequireJs - easily generate require-string in Razor
 * BasicCheckBoxFor - plain old checkbox, not the ordinary MVC-checkbox-plus-hidden-field

Static content
 * all the static content can be retrieved from StaticContent folder
 * URL rewriting is enabled for that matter, see web.config/system.webServer
 * URL is /Static/...

TODO
* Microsoft.AspNet.Identity.Core
* Microsoft.AspNet.Identity.Owin
* Owin
* Modernizr - ?

Installation
* after database restore, create/fix user
  - EXEC sp_change_users_login 'Auto_Fix', 'MessagesDbLocalUser'
* to enable SSL in Svc project, see readme and change bindings appropriately