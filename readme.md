To use NLog.Gelf just add the following to your config file and place NLog.Gelf.dll in the same location as the NLog.dll file:

```
<nlog>
    <extensions>
        <add assembly="NLog.Gelf" />
    </extensions>

    <targets>
        <target name="Gelf" type="GelfHttp" serverUrl="http://localhost:12202"/>
    </targets>

    <rules>
        <logger name="*" minLevel="Trace" appendTo="Gelf"/>
    </rules>
</nlog>
```