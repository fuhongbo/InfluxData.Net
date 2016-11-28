﻿using System;
using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb.ClientModules;
using InfluxData.Net.InfluxDb.Formatters;
using InfluxData.Net.InfluxDb.Infrastructure;
using InfluxData.Net.InfluxDb.QueryBuilders;
using InfluxData.Net.InfluxDb.RequestClients;
using InfluxData.Net.InfluxDb.ResponseParsers;
using InfluxData.Net.Common.Infrastructure;
using System.Net.Http;
using InfluxData.Net.InfluxDb.ClientSubModules;

namespace InfluxData.Net.InfluxDb
{
    public class InfluxDbClient : IInfluxDbClient
    {
        private readonly IInfluxDbRequestClient _requestClient;

        private readonly Lazy<ISerieQueryBuilder> _serieQueryBuilder;
        private readonly Lazy<IDatabaseQueryBuilder> _databaseQueryBuilder;
        private readonly Lazy<IRetentionQueryBuilder> _retentionQueryBuilder;
        private readonly Lazy<ICqQueryBuilder> _cqQueryBuilder;
        private readonly Lazy<IDiagnosticsQueryBuilder> _diagnosticsQueryBuilder;
        private readonly Lazy<IUserQueryBuilder> _userQueryBuilder;

        private readonly Lazy<IBasicResponseParser> _basicResponseParser;
        private readonly Lazy<ISerieResponseParser> _serieResponseParser;
        private readonly Lazy<IDatabaseResponseParser> _databaseResponseParser;
        private readonly Lazy<IRetentionResponseParser> _retentionResponseParser;
        private readonly Lazy<ICqResponseParser> _cqResponseParser;
        private readonly Lazy<IDiagnosticsResponseParser> _diagnosticsResponseParser;
        private readonly Lazy<IUserResponseParser> _userResponseParser;

        private readonly Lazy<IBasicClientModule> _basicClientModule;
        public IBasicClientModule Client
        { 
            get { return _basicClientModule.Value; }
        }

        private readonly Lazy<ISerieClientModule> _serieClientModule;
        public ISerieClientModule Serie
        {
            get { return _serieClientModule.Value; }
        }

        private readonly Lazy<IDatabaseClientModule> _databaseClientModule;
        public IDatabaseClientModule Database
        {
            get { return _databaseClientModule.Value; }
        }

        private readonly Lazy<IRetentionClientModule> _retentionClientModule;
        public IRetentionClientModule Retention
        {
            get { return _retentionClientModule.Value; }
        }

        private readonly Lazy<ICqClientModule> _cqClientModule;
        public ICqClientModule ContinuousQuery
        {
            get { return _cqClientModule.Value; }
        }

        private readonly Lazy<IDiagnosticsClientModule> _diagnosticsClientModule;
        public IDiagnosticsClientModule Diagnostics
        {
            get { return _diagnosticsClientModule.Value; }
        }

        public InfluxDbClient(string uri, string username, string password, InfluxDbVersion influxVersion, HttpClient httpClient = null, bool throwOnWarning = false)
             : this(new InfluxDbClientConfiguration(new Uri(uri), username, password, influxVersion, httpClient, throwOnWarning))
        private readonly Lazy<IUserClientModule> _userClientModule;
        public IUserClientModule User
        {
            get { return _userClientModule.Value; }
        }

        public InfluxDbClient(string uri, string username, string password, InfluxDbVersion influxVersion, HttpClient httpClient = null)
             : this(new InfluxDbClientConfiguration(new Uri(uri), username, password, influxVersion, httpClient))
        {
        }

        public InfluxDbClient(IInfluxDbClientConfiguration configuration)
        {
            var requestClientFactory = new InfluxDbClientBootstrap(configuration);
            var dependencies = requestClientFactory.GetClientDependencies();
            _requestClient = dependencies.RequestClient;

            // NOTE: once a breaking change occures, QueryBuilders will need to be resolved with factories
            _serieQueryBuilder = new Lazy<ISerieQueryBuilder>(() => new SerieQueryBuilder(), true);
            _databaseQueryBuilder = new Lazy<IDatabaseQueryBuilder>(() => new DatabaseQueryBuilder(), true);
            _retentionQueryBuilder = new Lazy<IRetentionQueryBuilder>(() => new RetentionQueryBuilder(), true);
            _cqQueryBuilder = new Lazy<ICqQueryBuilder>(() => dependencies.CqQueryBuilder, true);
            _diagnosticsQueryBuilder = new Lazy<IDiagnosticsQueryBuilder>(() => new DiagnosticsQueryBuilder(), true);
            _userQueryBuilder = new Lazy<IUserQueryBuilder>(() => new UserQueryBuilder(), true);

            // NOTE: once a breaking change occures, Parsers will need to be resolved with factories
            _basicResponseParser = new Lazy<IBasicResponseParser>(() => new BasicResponseParser(), true);
            _serieResponseParser = new Lazy<ISerieResponseParser>(() => dependencies.SerieResponseParser, true);
            _databaseResponseParser = new Lazy<IDatabaseResponseParser>(() => new DatabaseResponseParser(), true);
            _retentionResponseParser = new Lazy<IRetentionResponseParser>(() => new RetentionResponseParser(), true);
            _cqResponseParser = new Lazy<ICqResponseParser>(() => new CqResponseParser(), true);
            _diagnosticsResponseParser = new Lazy<IDiagnosticsResponseParser>(() => new DiagnosticsResponseParser(), true);
            _userResponseParser = new Lazy<IUserResponseParser>(() => new UserResponseParser(), true);

            // NOTE: once a breaking change occures, ClientModules will need to be resolved with factories
            _basicClientModule = new Lazy<IBasicClientModule>(() => new BasicClientModule(_requestClient, _basicResponseParser.Value));
            var batchWriter = new Lazy<IBatchWriterFactory>(() => new BatchWriter(_basicClientModule.Value));

            _serieClientModule = new Lazy<ISerieClientModule>(() => new SerieClientModule(_requestClient, _serieQueryBuilder.Value, _serieResponseParser.Value, batchWriter.Value));
            _databaseClientModule = new Lazy<IDatabaseClientModule>(() => new DatabaseClientModule(_requestClient, _databaseQueryBuilder.Value, _databaseResponseParser.Value));
            _retentionClientModule = new Lazy<IRetentionClientModule>(() => new RetentionClientModule(_requestClient, _retentionQueryBuilder.Value, _retentionResponseParser.Value));
            _cqClientModule = new Lazy<ICqClientModule>(() => new CqClientModule(_requestClient, _cqQueryBuilder.Value, _cqResponseParser.Value));
            _diagnosticsClientModule = new Lazy<IDiagnosticsClientModule>(() => new DiagnosticsClientModule(_requestClient, _diagnosticsQueryBuilder.Value, _diagnosticsResponseParser.Value));
            _userClientModule = new Lazy<IUserClientModule>(() => new UserClientModule(_requestClient, _userQueryBuilder.Value, _userResponseParser.Value));
        }

        public IPointFormatter GetPointFormatter()
        {
            return _requestClient.GetPointFormatter();
        }
    }
}