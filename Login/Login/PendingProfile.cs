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

/***
* PendingProfile.cs: Activity shows the profile for previously selected user.
*
* OnCreate: Initializes and sets activity view to display buttons and textviews. 
*
* BtnDeny_Click: Event handler sends a delete request to web api to delete the pending friendship when denied, 
*                event handler also displays textview to screen
* 
* BtnConfirm_Click: Event handler that adds friend to current users friend list through web api 
* 
* MakePostRequest: Sets the post request sent to the web api to add the new friend
* 
* MakeDeleteRequest: Sets the delete request sent to the web api to delete the pending friend from list 
* 
***/
namespace Login
{
    [Activity(Label = "PendingProfile")]
    public class PendingProfile : Activity
    {
        // initialize variables 
        private static dynamic friend;
        private TextView tvPPName;
        private string AccessToken;
        private TextView tvPPError;
        private TextView tvPPEmail;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PendingProfile);

            // set views
            tvPPError = (TextView)FindViewById(Resource.Id.tvPPError);
            tvPPName = (TextView)FindViewById(Resource.Id.tvPPName);
            tvPPEmail = (TextView)FindViewById(Resource.Id.tvPPEmail);

            // get the access token and person
            AccessToken = Intent.GetStringExtra("token");
            string serializedFriend = Intent.GetStringExtra("friend");

            // save friend into a dynamic to access later 
            friend = JsonConvert.DeserializeObject(serializedFriend);

            // show the first, last and username of user 
            tvPPName.Text = friend.FirstName + ' ' + friend.LastName;
            tvPPEmail.Text = friend.userName;

            // wire connect buttons to confirm and deny friendships
            Button btnConfirm = (Button)FindViewById(Resource.Id.btnConfirm);
            btnConfirm.Click += BtnConfirm_Click;
            Button btnDeny = (Button)FindViewById(Resource.Id.btnDeny);
            btnDeny.Click += BtnDeny_Click;
        }

        private async void BtnDeny_Click(object sender, EventArgs e)
        {
            //set url to access friends 
            string url = GetString(Resource.String.IP) + "api/friends/" + friend.newFriendId;
            string response = await MakeDeleteRequest(url, true);

            // show textview indicating pending friend request was denied
            if (response != null)
            {
                Toast.MakeText(this, "Friendship Denied/Cancelled", ToastLength.Short).Show();
                Finish();
            }
        }

        private async void BtnConfirm_Click(object sender, EventArgs e)
        {
            //set url to access friends and add new friend 
            string url = GetString(Resource.String.IP) + "api/friends";
            string payload = "{" + "newFriendId :" + JsonConvert.SerializeObject(friend.newFriendId) + "}";

            try
            {
                // send post request to web api to add new friend 
                string response = await MakePostRequest(url, payload, true);
                dynamic reply = JsonConvert.DeserializeObject(response);
                if (reply == null)
                {
                    Toast.MakeText(this, "Friendship Confirmed", ToastLength.Short).Show();
                    Finish();

                }
                else
                {
                    Toast.MakeText(this, "Waiting for friend to confirm", ToastLength.Long).Show();

                }
            }
            catch
            {
                tvPPError.Text = "ERROR SENDING CONFIRMATION";
            }

        }

        public async Task<string> MakePostRequest(string url, string serializedDataString, bool isJson)
        {
            // initialize http request and set url to passed string
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (isJson)
                request.ContentType = "application/json";
            else
                request.ContentType = "application/x-www-form-urlencoded";

            // set post method
            request.Method = "POST";
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(serializedDataString);
                writer.Flush();
                writer.Dispose();
            }

            // initialize variable to get specified response from request 
            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();

            // read data from stream
            using (StreamReader sr = new StreamReader(respStream))
            {
                return sr.ReadToEnd();
            }
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

            // initialize stream reader to previous request 
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
                writer.Flush();
                writer.Dispose();
            }

            // initialize variable to get specified response from request 
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
