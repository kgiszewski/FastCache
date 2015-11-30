FastCache
=========

Fast Cache for Umbraco

v2.1 (30th Nov 2015)

* Fixed rare issue; writing to the cache file while it was being read concurrently in another thread. 

v2.0 (25th Nov 2015)

* The content type of the output is set to text/html, this was the most important thing that was broken with the first version. I was getting plain text pages with Chrome. 
* Partially re-written and cleaned up. There was a lot of code cruft and debug writes etc
* Removed the "spider" component and aspx files
* Config strings are now in a resource file
* The events code now works on recent Umbraco versions (using the 6+ api) and I have tested it deleting files from the cache after a publish
* As well as caching to the file system it also caches the html into the application cache and doesn't even touch the file system after the initial check. This speeds it up a lot especially on Azure-hosted sites because file system access is slower than on a native box.
* It will delete the specific cache files that changed

Installation instructions

- Download the package (2.1) from: https://drive.google.com/a/developer-x.com/file/d/0Bz9S4LIuPplkVjlnV0JjT2d0dlU/view?usp=sharing
- Install package
- Add the following to your web.config under system.webServer->modules (mind where you put this if you use rewrites, etc). Sorry I tried to figure out how to automate this and didn't get anywhere. 

```html
<add name="FastCache" type="FastCache.CacheModule, FastCache" />
```

Notes

- You may bypass and refresh the cache on any single page by adding "no-cache" into the request url i.e. /home/?no-cache

Authors

- Kevin Giszewski (v1.0)
- Tim Scarfe (v2.0)