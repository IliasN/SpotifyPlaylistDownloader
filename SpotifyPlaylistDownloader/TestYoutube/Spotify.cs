/**
 * Name : Ilias N'hairi
 * Version : 1.0
 * Date : 22.02.2017
 * Liscence : GNU GENERAL PUBLIC LICENSE Version 3
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace SpotifyPlaylistDownloader
{
    class Spotify
    {
        #region Fields
        private SpotifyWebAPI _spotify;
        private PrivateProfile _profile;
        private List<SimplePlaylist> _playlists;
        private string _id;


        #endregion

        #region Properties
        private string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        #region Methods

        #region Constructor
        public Spotify(string id)
        {
            this.Id = id;
            _spotify = new SpotifyWebAPI();
            RunAuthentication();
        }
        #endregion

        /// <summary>
        /// Get the name of all tracks from a playlist
        /// </summary>
        /// <param name="id">the id of the playlist</param>
        /// <returns>List of tracks name</returns>
        public List<PlaylistTrack> GetPlaylistTrackById(int id)
        {
            Paging<PlaylistTrack> tracks = _spotify.GetPlaylistTracks(this.Id, _playlists[id].Id);
            List<PlaylistTrack> pt = tracks.Items.ToList();
            return pt;
        }

        /// <summary>
        /// Get all the names of the playlists
        /// </summary>
        /// <returns>List of all the names of the playlists</returns>
        public List<string> GetPlaylistsNames()
        {
            List<string> names = new List<string>();
            foreach (var item in _playlists)
            {
                names.Add(item.Name);
            }
            return names;
        }

        /// <summary>
        /// Get all the playlists from a spotify account
        /// </summary>
        /// <returns>A list of playlists</returns>
        private List<SimplePlaylist> GetPlaylists()
        {
            Paging<SimplePlaylist> playlists = _spotify.GetUserPlaylists(this.Id);
            List<SimplePlaylist> list = playlists.Items.ToList();

            while (playlists.Next != null)
            {
                playlists = _spotify.GetUserPlaylists(this.Id, 20, playlists.Offset + playlists.Limit);
                list.AddRange(playlists.Items);
            }
            list = list.Where(p => p.Collaborative == false && p.Owner.Id == this.Id).ToList();
            return list;
        }

        /// <summary>
        /// Log the user with the spotify API
        /// </summary>
        private async void RunAuthentication()
        {
            WebAPIFactory webApiFactory = new WebAPIFactory(
                "http://localhost",
                8000,
                "55ee327b5ce14372b266499ff64c2e57",
                Scope.PlaylistReadPrivate);

            try
            {
                _spotify = await webApiFactory.GetWebApi();
            }
            catch (Exception)
            {
            }

            if (_spotify == null)
                return;

            InitialSetup();
        }

        /// <summary>
        /// Setup the profil and the playlists
        /// </summary>
        private void InitialSetup()
        {
            _profile = _spotify.GetPrivateProfile();

            _playlists = GetPlaylists();
        }

        #endregion
    }
}
