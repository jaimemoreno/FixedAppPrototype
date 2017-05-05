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
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Provider;
/***
* FriendProfile.cs Activity: 
*
* 
* OnCreate: Initializes and sets activity view to be displayed.
*
* BtnSMS_Click: Starts sms messenger to send text message to selected friend.
*
* BtnFPRemove_Click: Removes friend from current friends list. 
*
* BtnFPEmail_Click: Starts android email application and sets title.
* 
* MakeDeleteRequest: Sets the http delete request sent to the web api. 
* 
***/


namespace Login
{
    [Activity(Label = "FriendProfile")]
    public class FriendProfile : Activity
    {
        private static dynamic friend;
        private TextView tvFPName;
        private string AccessToken;
        private TextView tvFPError; 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.FriendProfile);

            // Link the views
            tvFPName = (TextView)FindViewById(Resource.Id.tvFPName);
            TextView tvFPEmail = (TextView)FindViewById(Resource.Id.tvFPEmail);
            TextView tvFPPhone = (TextView)FindViewById(Resource.Id.tvFPPhone);
            Button btnFPEmail = (Button)FindViewById(Resource.Id.btnFPEmail);
            Button btnFPRemove = (Button)FindViewById(Resource.Id.btnFPRemove);
            Button btnSMS = (Button)FindViewById(Resource.Id.btnSMS);
            tvFPError = (TextView) FindViewById(Resource.Id.tvFPError);

            // Get the serialization
            string serializedFriend = Intent.GetStringExtra("friend");

            // save deserialized data to dynamic 
            friend = JsonConvert.DeserializeObject(serializedFriend);

            // get extra from previous activity
            AccessToken = Intent.GetStringExtra("token");

            // set views to display friend info
            tvFPName.Text = friend.FirstName + ' ' + friend.LastName;
            tvFPEmail.Text = friend.userName;
            tvFPPhone.Text = friend.PhoneNumber;

            // allow sms to friend if friend has phone number saved 
            if (friend.PhoneNumber != null)
            {
                btnSMS.Visibility = ViewStates.Visible;
            }

            btnFPEmail.Click += BtnFPEmail_Click;
            btnFPRemove.Click += BtnFPRemove_Click;
            btnSMS.Click += BtnSMS_Click;
        }

        private void BtnSMS_Click(object sender, EventArgs e)
        {
            // send text message to selected phone number 
            var smsUri = Android.Net.Uri.Parse("smsto:" + friend.PhoneNumber);
            var smsIntent = new Intent(Intent.ActionSendto, smsUri);
            smsIntent.PutExtra("sms_body", "");

            // start new activity 
            StartActivity(smsIntent);
        }

        private async void BtnFPRemove_Click(object sender, EventArgs e)
        {
           

            try
            {
                // make friend delete request to api and return response
                string url = GetString(Resource.String.IP) + "api/friends/" + friend.newFriendId;
                string response = await MakeDeleteRequest(url, true);
                Finish();
            }
            catch 
            {
                tvFPError.Text = "ERROR"; 
            }
        }

        private void BtnFPEmail_Click(object sender, EventArgs e)
        {
            // set new intent for email 
            var email = new Intent(Android.Content.Intent.ActionSend);

            // send extras to activity 
            email.PutExtra(Android.Content.Intent.ExtraEmail,
                new string[] { friend.userName });
            email.PutExtra(Android.Content.Intent.ExtraSubject, "PartyUp - ");
            email.SetType("message/rfc822");

            // start new activity
            StartActivity(email);           
        }

        public async Task<string> MakeDeleteRequest(string url, bool isJson)
        {
            // initialize http request and set url to passed string 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            // set http method to delete 
            request.Method = "DELETE";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
                writer.Flush();
                writer.Dispose();
            }

            // initialize stream reader 
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            // read data from stream
            using (StreamReader sr = new StreamReader(respStream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
