#nullable enable warnings
using API.Attributes;
using System.Net;

/*
 * This file contains all uses of the API.Attributes classes.
 * 
 * Redirect example: (redirects '/login.html' to '/login')
 *  [assembly : Redirect("/login.html", "/login", ValidOn = ServerAttributeTargets.HTML)]
 *
 * Alias example: (interprets a request to '/login' as a request to '/login.html')
 *  [assembly : Alias("/login.html", "/login", ValidOn = ServerAttributeTargets.HTML)]
 *  
 *  Notes:
 *  <value1> | <value2>		This copies all high bits from value2 to value1.		(aka. bitwise set)
 *  <value1> & ~<value2>	This sets all high bits from value2 to low in value1.	(aka. bitwise unset)
 */

#region Redirects

#endregion

#region Aliases

#endregion

#region Hidden Urls
[assembly: Alias("/errors/404.html", null, InvalidOn = ServerAttributeTargets.JSON)]
[assembly: Alias("/unauthorized", null, InvalidOn = ServerAttributeTargets.JSON)]
#endregion

#region Errorpages
[assembly: ErrorPage(HttpStatusCode.NotFound, "/errors/404.html")]
[assembly: ErrorPage(HttpStatusCode.Unauthorized, "/unauthorized", KeepStatusCode = false)]
#endregion

#nullable disable warnings