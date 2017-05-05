using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Login.Resources.layout;
using Newtonsoft.Json;
using Object = Java.Lang.Object;
/***
* Friends.cs Activity: Displays list of friends for current user. 
*
* 
* BtnAddFriends_Click: Displays AddFriend Activity and passes data through extras.
* 
* OnResume: Overrided function that displays the friends list after activity is resumed. 
*
* ListFriends_ItemClick: Displays the FriendProfile Activity and passes extras .
*
* BtnPending_Click: Sends activity to PendingFriends Activity and passes data. 
* 
* MakeGetRequest: Receives data from web api through http get request and returns the content received. 
* 
***/

namespace Login
{
    [Activity(Label = "Friends")]
    public class Friends : Activity
    {
        private static string AccessToken;
        TextView tvFriendsTest;
        ListView listFriends;
        private dynamic jsonData;
        private static string serializedResponse;
        SortedList<string, Friend> peopleSortedList = new SortedList<string, Friend>();
        List<string> names = new List<string>();

        protected  async override void OnCreate(Bundle savedInstanceState)
        {
            this.Title = "Friends List";
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Friends);

            // get accesstoken
            AccessToken = Intent.GetStringExtra("token");

            // wire up textviews
            tvFriendsTest = (TextView)FindViewById(Resource.Id.tvFriendsTest);
            listFriends = (ListView) FindViewById(Resource.Id.listViewFriends);
            listFriends.FastScrollEnabled = true;
            string url = GetString(Resource.String.IP) + "api/friends";

            // Get friends list
            try
            {
                
                serializedResponse = await MakeGetRequest(url);
                jsonData = JsonConvert.DeserializeObject(serializedResponse);
            }
            catch (Exception e)
            {
                tvFriendsTest.Text = "ERROR"; 
            }

            if (jsonData == null)
            {
                tvFriendsTest.Text = "ERROR";
            }


            //loop through each friend and put their data into a sorted list by first and last name
            // then work on seperating the people into confirmed and pending
            peopleSortedList.Clear();
            names.Clear();

            foreach (var x in jsonData)
            {
                if (x.friendshipEstablished == true.ToString())
                {
                    Friend person = new Friend();
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.PhoneNumber = x.PhoneNumber;
                    person.userName = x.userName;
                    person.friendshipEstablished = x.friendshipEstablished;
                    person.newFriendId = x.newFriendId;

                    peopleSortedList.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);

                    //popluate names and organize it. then you can call the corresponding key from the sorted list
                    names.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName);
                }



            }

            names.Sort();
       
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, names);

            // Bind the adapter to the ListView.
            listFriends.Adapter = adapter;
            listFriends.ItemClick += ListFriends_ItemClick;

            // wire up buttons 
            Button btnAddFriends = (Button) FindViewById(Resource.Id.btnAddFriends);
            btnAddFriends.Click += BtnAddFriends_Click;
            Button btnPending = (Button)FindViewById(Resource.Id.btnPending);
            btnPending.Click += BtnPending_Click;
        }

        private void BtnAddFriends_Click(object sender, EventArgs e)
        {
            // initialize intent for new activity and pass extra 
            Intent toAddFriends = new Intent(this, typeof(AddFriends));
            toAddFriends.PutExtra("friends", serializedResponse); 
            toAddFriends.PutExtra("token", AccessToken); 
            StartActivity(toAddFriends);
        }

        protected async override void OnResume()
        {
            base.OnResume();

            string url = GetString(Resource.String.IP) + "api/friends";
            //Get friends list
            try
            {

                serializedResponse = await MakeGetRequest(url);
                jsonData = JsonConvert.DeserializeObject(serializedResponse);
            }
            catch
            {
                tvFriendsTest.Text = "ERROR";
            }

            if (jsonData == null)
            {
                tvFriendsTest.Text = "ERROR";
            }


            //loop through each friend and put their data into a sorted list by first and last name
            // then work on seperating the people into confirmed and pending
            peopleSortedList.Clear();
            names.Clear();

            //populate all established friends
            foreach (var x in jsonData)
            {
                if (x.friendshipEstablished == true.ToString())
                {
                    Friend person = new Friend();
                    person.FirstName = x.FirstName;
                    person.LastName = x.LastName;
                    person.PhoneNumber = x.PhoneNumber;
                    person.userName = x.userName;
                    person.friendshipEstablished = x.friendshipEstablished;
                    person.newFriendId = x.newFriendId;

                    peopleSortedList.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName, person);

                    //populuate names and organize it. then you can call the corresponding key from the sorted list
                    names.Add(person.FirstName + ' ' + person.LastName + " - " + person.userName);
                }

            }

            names.Sort();
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, names);

            // Bind the adapter to the ListView.
            listFriends.Adapter = adapter;
        }

        private void BtnPending_Click(object sender, EventArgs e)
        {
            // initialize intent for new activity 
            Intent toPendingFriends = new Intent(this, typeof(PendingFriends));

            // pass data to new activities through extras 
            toPendingFriends.PutExtra("token", AccessToken);
            toPendingFriends.PutExtra("response", serializedResponse);
            StartActivity(toPendingFriends);
        }

        private void ListFriends_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // initialize intent for new activity 
            Intent toFriendProfile = new Intent(this, typeof(FriendProfile));
            var selectedFriend = peopleSortedList[names[e.Position]];
            string serializedFriend = JsonConvert.SerializeObject(selectedFriend);

            // pass data to new activities through extras 
            toFriendProfile.PutExtra("friend", serializedFriend);
            toFriendProfile.PutExtra("token", AccessToken);
            StartActivity(toFriendProfile);
        }

        public static async Task<string> MakeGetRequest(string url)
        {
            //initialize http request and set method to a get request 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            //initialize variables to stream data from web api
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            respStream.Flush();

            // read content that was returned from the get request
            using (StreamReader sr = new StreamReader(respStream))
            {            
                string strContent = sr.ReadToEnd();
                respStream = null;
                return strContent;
            }
        }
    }
}
