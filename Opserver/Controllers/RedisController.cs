﻿using System.Web.Mvc;
using StackExchange.Opserver.Data.Redis;
using StackExchange.Opserver.Helpers;
using StackExchange.Opserver.Models;
using StackExchange.Opserver.Views.Redis;

namespace StackExchange.Opserver.Controllers
{
    [OnlyAllow(Roles.Redis)]
    public partial class RedisController : StatusController
    {
        protected override ISecurableSection SettingsSection
        {
            get { return Current.Settings.Redis; }
        }

        [Route("redis")]
        public ActionResult Dashboard(string node)
        {
            var instance = RedisInstance.GetInstance(node);
            if (instance != null)
                return RedirectToAction("Instance", new {node});
            if (node.HasValue())
                return ServerView(node);

            var vd = new DashboardModel
            {
                Instances = RedisInstance.AllInstances,
                View = node.HasValue() ? DashboardModel.Views.Server : DashboardModel.Views.All,
                CurrentRedisServer = node,
                Refresh = true
            };
            return View("Dashboard", vd);
        }

        [Route("redis/server")]
        public ActionResult ServerView(string node)
        {
            if (node == null)
                return RedirectToAction("Dashboard");

            var vd = new DashboardModel
            {
                Instances = RedisInstance.AllInstances,
                View = node.HasValue() ? DashboardModel.Views.Server : DashboardModel.Views.All,
                CurrentRedisServer = node,
                Refresh = true
            };
            return View("Dashboard", vd);
        }

        [Route("redis/instance")]
        public ActionResult Instance(string node, bool ajax = false)
        {
            var instance = RedisInstance.GetInstance(node);

            var vd = new DashboardModel
            {
                Instances = RedisInstance.AllInstances,
                CurrentInstance = instance,
                View = DashboardModel.Views.Instance,
                CurrentRedisServer = node,
                Refresh = true
            };
            return View(ajax ? "Instance" : "Dashboard", vd);
        }

        [Route("redis/instance/summary/{type}")]
        public ActionResult InstanceSummary(string node, int port, string type)
        {
            var i = RedisInstance.GetInstance(node, port);
            if (i == null) return ContentNotFound("Could not find instance " + node + ":" + port);

            switch (type)
            {
                case "clients":
                    return View("Instance.Clients", i);
                default:
                    return ContentNotFound("Unknown summary view requested");
            }
        }

        [Route("redis/analyze/memory")]
        public ActionResult Analysis(string node, int port, int db, bool runOnMaster = false)
        {
            var rci = RedisConnectionInfo.FromCache(node, port);
            if (rci == null)
                return TextPlain("Connection not found");
            var analysis = RedisAnalyzer.AnalyzerDatabaseMemory(rci, db, runOnMaster);

            return View("Server.Analysis.Memory", analysis);
        }

        [Route("redis/analyze/memory/clear")]
        public ActionResult ClearAnalysis(string node, int port, int db)
        {
            var rci = RedisConnectionInfo.FromCache(node, port);
            if (rci == null)
                return TextPlain("Connection not found");
            RedisAnalyzer.ClearDatabaseMemoryAnalysisCache(rci, db);

            return RedirectToAction("Analysis", new { node, port, db });
        }
    }
}
