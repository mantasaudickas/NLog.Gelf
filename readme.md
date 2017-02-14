Extension is developed using .NET core app 1.0

Installation:
```
dotnet add package NLog.Gelf
```
OR
```
Install-Package NLog.Gelf -Pre
```


To use NLog.Gelf just add the following to your config file and place NLog.Gelf.dll in the same location as the NLog.dll file:

```
<nlog>
    <extensions>
        <add assembly="NLog.Gelf" />
    </extensions>

    <targets>
        <target name="Gelf" type="GelfHttp" serverUrl="http://localhost:12201/gelf" facility="your app name"/>
    </targets>

    <rules>
        <logger name="*" minLevel="Trace" appendTo="Gelf"/>
    </rules>
</nlog>
```

Tested with graylog 2.1.1 server docker container.
docker-compose.yml file:

```
version: '2'
services:
    graylog-mongo:
      image: "mongo:3"
      container_name: graylog-mongo
    graylog-elasticsearch:
      image: "elasticsearch:2"
      container_name: graylog-elasticsearch
      command: "elasticsearch -Des.cluster.name='graylog'"
    graylog:
      image: graylog2/server:2.1.1-1
      container_name: graylog
      environment:
        GRAYLOG_PASSWORD_SECRET: somepasswordpepper
        GRAYLOG_ROOT_PASSWORD_SHA2: 415e8a6ba1c3eb93e81df34731acc3d60efee685c8e6f7412592a45ba3a0e3b0
        GRAYLOG_WEB_ENDPOINT_URI: http://127.0.0.1:9000/api
      links:
        - graylog-elasticsearch:elasticsearch
        - graylog-mongo:mongo
      ports:
        - "9000:9000"
        - "12201:12201"
        - "1514:1514"
```

In order to start container execute command in the same folder as docker-compose.yml file is stored:

```
docker-compose up -d
```

Here it uses default admin password. Please change it to your strong password.
When container starts, access it using: http://localhost:9000/dashboard.
You need to configure GELF HTTP input on port 12202 for this logger (port can be different and specified in docker-compose.yml and nlog.config).