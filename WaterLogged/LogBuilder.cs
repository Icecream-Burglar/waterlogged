﻿using System;
using System.Collections.Generic;
using System.Text;
using WaterLogged.Filters;
using WaterLogged.Formatting;

namespace WaterLogged
{
    public class LogBuilder
    {
        public enum Contexts
        {
            Log,
            Listener,
            Sink,
        }

        public Contexts Context { get; set; }
        public FilterManager LogFilter { get; set; }
        public LogicalFormatter Formatter { get; private set; }
        public Listener LastListener { get; private set; }
        public TemplatedMessageSink LastSink { get; private set; }

        private string _logName;
        private List<Listener> _listeners;
        private List<TemplatedMessageSink> _messageSinks;

        private object _globalKey;
        private bool _globalPrimary;
        

        public LogBuilder()
        {
            _logName = "";
            _listeners = new List<Listener>();
            _messageSinks = new List<TemplatedMessageSink>();
            Context = Contexts.Log;
            Formatter = new LogicalFormatter();
            LogFilter = new FilterManager();

            _globalKey = null;
            _globalPrimary = false;
        }

        public LogBuilder WithListener(Listener listener, string name = "")
        {
            listener.Name = name;
            _listeners.Add(listener);
            LastListener = listener;
            Context = Contexts.Listener;
            return this;
        }

        public LogBuilder WithSink(TemplatedMessageSink sink, string name = "")
        {
            sink.Name = name;
            _messageSinks.Add(sink);
            LastSink = sink;
            Context = Contexts.Sink;
            return this;
        }

        public LogBuilder WithFormatString(string format)
        {
            Formatter.Format = format;
            return this;
        }

        public LogBuilder WithFormatVariable(string key, string value)
        {
            Formatter.Variables.Add(key, value);
            return this;
        }

        public LogBuilder WithFormatFunc(string name, Delegate func)
        {
            Formatter.BaseContext.Functions.Add(name, func);
            return this;
        }

        public LogBuilder Log(string name = "")
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _logName = name;
            }
            Context = Contexts.Log;
            return this;
        }

        public LogBuilder WithName(string name)
        {
            switch (Context)
            {
                case Contexts.Log:
                    _logName = name;
                    break;
                case Contexts.Listener:
                    LastListener.Name = name;
                    break;
                case Contexts.Sink:
                    LastSink.Name = name;
                    break;
            }
            return this;
        }

        public LogBuilder WithFilter(IFilter filter)
        {
            switch (Context)
            {
                case Contexts.Log:
                    LogFilter.Filters.Add(filter);
                    break;
                case Contexts.Listener:
                    LastListener.Filter.Filters.Add(filter);
                    break;
                case Contexts.Sink:
                    LastSink.Filter.Filters.Add(filter);
                    break;
            }
            return this;
        }

        public LogBuilder WithFilter(FilterPredicate filter)
        {
            switch (Context)
            {
                case Contexts.Log:
                    LogFilter.Filters.Add(new DelegatedFilter(filter));
                    break;
                case Contexts.Listener:
                    LastListener.Filter.Filters.Add(new DelegatedFilter(filter));
                    break;
                case Contexts.Sink:
                    LastSink.Filter.Filters.Add(new DelegatedFilter(filter));
                    break;
            }
            return this;
        }

        public LogBuilder WithTemplatedFilter(ITemplatedMessageFilter filter)
        {
            switch (Context)
            {
                case Contexts.Log:
                    LogFilter.TemplatedFilters.Add(filter);
                    break;
                case Contexts.Listener:
                    LastListener.Filter.TemplatedFilters.Add(filter);
                    break;
                case Contexts.Sink:
                    LastSink.Filter.TemplatedFilters.Add(filter);
                    break;
            }
            return this;
        }

        public LogBuilder WithTemplatedFilter(TemplatedFilterPredicate filter)
        {
            switch (Context)
            {
                case Contexts.Log:
                    LogFilter.TemplatedFilters.Add(new DelegatedFilter(filter));
                    break;
                case Contexts.Listener:
                    LastListener.Filter.TemplatedFilters.Add(new DelegatedFilter(filter));
                    break;
                case Contexts.Sink:
                    LastSink.Filter.TemplatedFilters.Add(new DelegatedFilter(filter));
                    break;
            }
            return this;
        }

        public LogBuilder WithGlobalKey(object key)
        {
            _globalKey = key;
            return this;
        }

        public LogBuilder AsGlobalPrimary()
        {
            _globalPrimary = true;
            return this;
        }


        public Log Build()
        {
            if (_globalPrimary)
            {
                Global.NextLogIsPrimary = true;
            }
            WaterLogged.Log log;
            if (string.IsNullOrWhiteSpace(_logName))
            {
                log = new Log();
            }
            else
            {
                log = new Log(_logName);
            }
            log.Filter = LogFilter;
            log.Formatter = Formatter;

            foreach (var listener in _listeners)
            {
                log.AddListener(listener);
            }

            foreach (var sink in _messageSinks)
            {
                log.AddSink(sink);
            }

            if (_globalKey != null)
            {
                Global.GlobalLogs.Add(_globalKey, log);
            }
            return log;
        }
    }
}