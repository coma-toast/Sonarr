﻿using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class IndexerProviderTest
    // ReSharper disable InconsistentNaming
    {
        [Test]
        public void Download_feed_test()
        {
            var mocker = new AutoMoqer();

            var xmlReader = XmlReader.Create(File.OpenRead(".\\Files\\Rss\\nzbsorg.xml"));

            mocker.GetMock<HttpProvider>()
                .Setup(h => h.DownloadXml(It.IsAny<String>()))
                .Returns(xmlReader);

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.Resolve<MockIndexerProvider>().Fetch();
        }

        [Test]
        public void Init_indexer_test()
        {
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var indexerProvider = mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerProviderBase>() { mocker.Resolve<MockIndexerProvider>() });
            var indexers = indexerProvider.All();

            //Assert
            Assert.Count(1, indexers);
        }

        [Test]
        public void unmapped_series_shouldnt_call_any_providers()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries(It.IsAny<String>()))
                .Returns<Series>(null);

            var indexer = mocker.Resolve<MockIndexerProvider>();
            indexer.ProcessItem(new SyndicationItem { Title = new TextSyndicationContent("Adventure.Inc.S01E18.DVDRip.XviD-OSiTV") });
        }
    }

    public class MockIndexerProvider : IndexerProviderBase
    {
        public MockIndexerProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IndexerProvider indexerProvider, HistoryProvider historyProvider, SabProvider sabProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, indexerProvider, historyProvider, sabProvider)
        {
        }

        protected override string[] Urls
        {
            get { return new[] { "www.google.com" }; }
        }

        public override string Name
        {
            get { return "Mocked Indexer"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }
    }
}