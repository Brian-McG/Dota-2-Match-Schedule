using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace Dota2MatchApp.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String id,
                               String teamOneTag,
                               String teamTwoTag,
                               String teamOneScore,
                               String teamTwoScore,
                               String teamOneLogoUrl,
                               String teamTwoLogoUrl,
                               String league,
                               String startTime,
                               String seriesType
                             )
        {
            this.UniqueId = id;
            this.TeamOneTag = teamOneTag;
            this.TeamTwoTag = teamTwoTag;
            this.TeamOneScore = teamOneScore;
            this.TeamTwoScore = teamTwoScore;
            this.TeamOneLogoUrl = teamOneLogoUrl;
            this.TeamTwoLogoUrl = teamTwoLogoUrl;
            this.League = league;
            this.StartTime = startTime;
            this.SeriesType = seriesType;
        }

        public string UniqueId { get; private set; }
        public string TeamOneTag { get; private set; }
        public string TeamTwoTag { get; private set; }
        public string TeamOneScore { get; private set; }
        public string TeamTwoScore { get; private set; }
        public string TeamOneLogoUrl { get; private set; }
        public string TeamTwoLogoUrl { get; private set; }
        public string League { get; private set; }
        public string StartTime { get; private set; }
        public string SeriesType { get; private set; }

        public override string ToString()
        {
            return this.UniqueId;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(string uniqueId)
        {
            this.UniqueId = uniqueId;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.UniqueId.ToString();
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            _groups.Clear();
            //System.Diagnostics.Debug.WriteLine("It gets here 1");
            UriBuilder uriBuilder = new UriBuilder("http", "ec2-54-229-206-249.eu-west-1.compute.amazonaws.com", 80, "match_api");
            Uri dataUri = uriBuilder.Uri;

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(dataUri);
            var result = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonObject.Parse(result);
            int timestamp = (int)jsonObject["timestamp"].GetNumber();
            SampleDataGroup group = new SampleDataGroup("Group-1");
            foreach (JsonValue itemValue in jsonObject["matches"].GetArray())
            {
                JsonObject itemObject = itemValue.GetObject();
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(itemObject["starttime_unix"].GetString())).ToLocalTime();
                const string prefixUrl = "http://dailydota2.com";
                group.Items.Add(new SampleDataItem(itemObject["link"].GetString(),
                                                   itemObject["team1"].GetObject()["team_tag"].GetString(),
                                                   itemObject["team2"].GetObject()["team_tag"].GetString(),
                                                   itemObject["team1"].GetObject()["score"].GetString(),
                                                   itemObject["team2"].GetObject()["score"].GetString(),
                                                   prefixUrl + itemObject["team1"].GetObject()["logo_url"].GetString(),
                                                   prefixUrl + itemObject["team2"].GetObject()["logo_url"].GetString(),
                                                   "League: " + itemObject["league"].GetObject()["name"].GetString(),
                                                   "Time: "+dtDateTime.ToString(),
                                                   "Type: Best of " + ((int)itemObject["series_type"].GetNumber()).ToString()));
            }
            this.Groups.Add(group);
            
        }
    }

}