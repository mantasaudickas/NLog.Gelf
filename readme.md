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