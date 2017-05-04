using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace Login
{
    [Activity(Label = "PendingFriends")]
    public class PendingFriends : Activity
    {
        //initialize variables
        private TextView tvPendingError;
        SortedList<string, Friend> pendingFriends = new SortedList<string, Friend>();
        private ListView lvPending;
        private static string AccessToken;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PendingFriends);

            //set views
            tvPendingError = (TextView)FindViewById(Resource.Id.tvPendingError);
            lvPending = (ListView)FindViewById(Resource.Id.lvPending);
            lvPending.FastScrollEnabled = true;

            // Receive access token from previous activity 
            AccessToken = Intent.GetStringExtra("token");
            string serializedResponse = Intent.GetStringExtra("response");

            dynamic jsonData = JsonConvert.DeserializeObject(serializedResponse);

            // loop through Json data to find if friendship is established 
            foreach (var x in jsonData)
            {
                
                if (x.friendshipEstablished == false.ToString())
                {
                    Friend person = new Friend();
                    person.newFriendId = x.newFriendId;
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.PhoneNumber = x.PhoneNumber;
                    person.userName = x.userName;
                    person.friendshipEstablished = x.friendshipEstablished;

                    pendingFriends.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);
                }

            }

            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, pendingFriends.Keys.ToArray());
            // Bind the adapter to the ListView.
            lvPending.Adapter = adapter;
            lvPending.ItemClick += LvPending_ItemClick;
        }

        //button to send to PendingProfile activity, activity goes to selected pending friend
        private void LvPending_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //set intent being passed to next activity 
            Intent toFriendProfile = new Intent(this, typeof(PendingProfile));
            var selectedFriend = pendingFriends[pendingFriends.Keys[e.Position]];
            string serializedFriend = JsonConvert.SerializeObject(selectedFriend);
            toFriendProfile.PutExtra("friend", serializedFriend);
            toFriendProfile.PutExtra("token", AccessToken);
            StartActivity(toFriendProfile);

        }

        // OnResume function is overrided to specify action when activity is returned to 
        protected async override void OnResume()
        {
            base.OnResume();
            string serializedResponse;
            dynamic jsonData;
            string url = GetString(Resource.String.IP) + "api/friends";
            pendingFriends.Clear();
            try
            {
                //accept serialized response 
                serializedResponse = await MakeGetRequest(url);

                //data is converted to json to be used in the loop 
                jsonData = JsonConvert.DeserializeObject(serializedResponse);

                //checks if any data was returned
                if (jsonData == null)
                {
                    tvPendingError.Text = "ERROR";
                }

                //loop through json data 
                foreach (var x in jsonData)
                {
                    //find each pending request by comparing the frienshipEstablished boolean
                    if (x.friendshipEstablished == false.ToString())
                    {
                        Friend person = new Friend();
                        person.newFriendId = x.newFriendId;
                        person.FirstName = x.FirstName;
                        person.LastName = x.LastName;
                        person.PhoneNumber = x.PhoneNumber;
                        person.userName = x.userName;
                        person.friendshipEstablished = x.friendshipEstablished;

                        pendingFriends.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);
                    }

                }
            }
            catch
            {
                tvPendingError.Text = "ERROR";
            }

           

            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, pendingFriends.Keys.ToArray());
            // Bind the adapter to the ListView.
            lvPending.Adapter = adapter;
        }
        
        // establishes the get request for the api 
        public static async Task<string> MakeGetRequest(string url)
        {
            //initialize http request and set method to a get request 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            //set variable to store response being routed by api 
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            // read content that was returned from the get request 
            using (StreamReader sr = new StreamReader(respStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                respStream = null;
                return strContent;
            }
        }
    }
}
