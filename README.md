**Project Description**

Create a set of .net utilities for Azure DocumentDB and expose them as a webapi and web site.


　

A utility library for Azure DocumentDB databases.  Original requirement was to be able to query multiple databases and containers.

[http://stuartmcleantech.blogspot.co.uk/2016/03/scalable-querying-multiple-azure.html](http://stuartmcleantech.blogspot.co.uk/2016/03/scalable-querying-multiple-azure.html)

Exposed as a simple web api service.

Next step is to put a simple web front end.


Current functionality can be accesses by web api (hosted on azure, sponsored by [Stiona Software Ltd.](http://www.stionasoftware.com)).


http://documentdbservices.azurewebsites.net/api/Document/?endpointUrl={YOUR ENDPOINT}&authorizationKey={YOUR KEY}&query={YOUR QUERY}


You must url encode the parameters.  I used this service - [http://www.url-encode-decode.com/](http://www.url-encode-decode.com/).

Actually - I only had to encode the authorizationKey - it contains a fair amount of special characters - everything else the browser was able to take care of.
