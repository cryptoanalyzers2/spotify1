using System;
using System.Threading.Tasks;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Collections.Generic;

namespace SpotifyAPI.Web.Examples.CLI
{
    internal static class Program
    {
        private static string _clientId = "b2931def6f7a47a981a43ea5c9c20c9e"; //"";
        private static string _secretId = "b482925b5391423d8f2466288819b822"; //"";

        /*
        Client ID b2931def6f7a47a981a43ea5c9c20c9e
        Client Secret b482925b5391423d8f2466288819b822
        */
        
        
        // ReSharper disable once UnusedParameter.Local
        public static void Main(string[] args)
        {
            _clientId = string.IsNullOrEmpty(_clientId) ?
              Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") :
              _clientId;

            _secretId = string.IsNullOrEmpty(_secretId) ?
              Environment.GetEnvironmentVariable("SPOTIFY_SECRET_ID") :
              _secretId;

            Console.WriteLine("####### Spotify API Example #######");
            Console.WriteLine("This example uses AuthorizationCodeAuth.");
            Console.WriteLine(
              "Tip: If you want to supply your ClientID and SecretId beforehand, use env variables (SPOTIFY_CLIENT_ID and SPOTIFY_SECRET_ID)");

            var auth =
              new AuthorizationCodeAuth(_clientId, _secretId, "http://localhost:4002", "http://localhost:4002",
                Scope.PlaylistReadPrivate | Scope.PlaylistReadCollaborative| Scope.PlaylistModifyPrivate|Scope.Streaming|Scope.AppRemoteControl|Scope.PlaylistModifyPublic| Scope.PlaylistReadPrivate| Scope.UserTopRead);
            auth.AuthReceived += AuthOnAuthReceived;

            


            auth.Start();
            auth.OpenBrowser();

            Console.ReadLine();
            auth.Stop(0);

        }

        private static async void AuthOnAuthReceived(object sender, AuthorizationCode payload)
        {
            var auth = (AuthorizationCodeAuth)sender;
            auth.Stop();

            Token token = await auth.ExchangeCode(payload.Code);
            var api = new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };
            await PrintUsefulData(api);
        }

        private static async Task PrintAllPlaylistTracks(SpotifyWebAPI api, Paging<SimplePlaylist> playlists)
        {
            if (playlists.Items == null) return;

            playlists.Items.ForEach(playlist => Console.WriteLine($"- {playlist.Name}"));
            if (playlists.HasNextPage())
                await PrintAllPlaylistTracks(api, await api.GetNextPageAsync(playlists));
        }

        private static async Task PrintUsefulData(SpotifyWebAPI api)
        {
            PrivateProfile profile = await api.GetPrivateProfileAsync();
            string name = string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName;
            Console.WriteLine($"Hello there, {name}!");

            Console.WriteLine("Your playlists:");
            await PrintAllPlaylistTracks(api, api.GetUserPlaylists(profile.Id));
            Console.WriteLine("Artist Track:");
            await testPlayArtistTrack(api);

            
        }



        private static async Task testPlayArtistTrack(SpotifyWebAPI api)
        {
              PrivateProfile profile = await api.GetPrivateProfileAsync();

            

        //    AvailabeDevices devices = api.GetDevices();
        //    devices.Devices.ForEach(device => Console.WriteLine(device.Name));


            var devices = api.GetDevices();
       
            if (devices.Devices != null)
            {
                var device_id = devices.Devices[0].Id;
                Console.WriteLine("Device id=" + device_id);
            }
            else
            {
                Console.WriteLine("no devices!");



            }

    
            Paging<SimpleTrack> tracks=api.GetAlbumTracks("2gcHtTSYMn8VhYlUdkI6kd");

            foreach (var track in tracks.Items)
            {

                Console.WriteLine("name=" + track.Name + " id=" + track.Id);
                //play it
            ;

                FullPlaylist list = api.CreatePlaylist(profile.Id, "test track");
                //   4iV5W9uYEdYUVa79Axb7Rh
                //    6MCSPhkasUOCC43gwP0Mqb

                //   ErrorResponse resp= api.AddPlaylistTrack(list.Id, "spotify:track:4iV5W9uYEdYUVa79Axb7Rh");

                //      Console.WriteLine("AddPlaylistTrack:" +resp.Error.Message.ToString()+resp.Error.Status);
                //     resp=api.PausePlayback();

                //       Console.WriteLine("PausePlayback:" + resp.Error.Message.ToString() + resp.Error.Status);

                ErrorResponse resp = api.ResumePlayback("", "", new List<string> { "spotify:track:6MCSPhkasUOCC43gwP0Mqb" }, "", 0);

                if(resp.HasError())
                Console.WriteLine("ResumePlayback:" + resp.Error.Message.ToString() + "\n"+resp.Error.Status);

            //    ErrorResponse error = api.ResumePlayback(uris: new List<string> { "spotify:track:4iV5W9uYEdYUVa79Axb7Rh" });

                var playback = api.GetPlayback();
                if(playback ==null)
                {

                    Console.WriteLine("no playback!");
                    return;

                }

                Console.WriteLine("is playing?" + playback.IsPlaying);
                if(playback.IsPlaying)
                    Console.WriteLine(" ... playing:" + playback.Item.Name);

            }
           

        }



    }
}
 