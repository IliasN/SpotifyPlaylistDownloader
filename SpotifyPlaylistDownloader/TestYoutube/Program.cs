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
using YoutubeSearch;
using System.IO;
using System.Diagnostics;
using MediaToolkit;
using MediaToolkit.Model;
using YoutubeExtractor;
using MediaToolkit.Options;
using TagLib;
using SpotifyAPI.Web.Models;

namespace SpotifyPlaylistDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            //Ask what account to retrieve playlists from
            Console.WriteLine("Enter the ID of the account you want to consult the playlists : ");
            Spotify spot = new Spotify(Console.ReadLine());

            //Get the id of the playlist do download
            var pl = spot.GetPlaylistsNames();

            int id = -1;
            while (id < 0 || id >= pl.Count())
            {
                Console.WriteLine("Select the playlist that you want to download : ");
                for (int i = 0; i < pl.Count(); i++)
                {
                    Console.WriteLine(i.ToString() + ". " + pl[i]);
                }
                string tmp = Console.ReadLine();
                int tmp2 = -1;
                if(int.TryParse(tmp,out tmp2))
                {
                    id = Convert.ToInt32(tmp);
                }
            }

            //Get the tracks of the selectioned playlist
            var playlistTracks = spot.GetPlaylistTrackById(id);

            //Get the name of the playlist
            string plName = pl[id].Trim();
            Console.WriteLine("Downloading : " + plName);

            //Create a folder with the name of the playlist of one doesn't already exists
            if (!System.IO.File.Exists(plName))
            {
                Directory.CreateDirectory(plName);
            }
            var items = new VideoSearch();
            //Is used to count the number of downloaded tracks
            int c = 1;

            //Goes through each song
            foreach (var song in playlistTracks)
            {
                string songName = ClearChars(song.Track.Name + " - " + song.Track.Artists[0].Name);
                foreach (var item in items.SearchQuery(songName, 1))
                {
                    string link = item.Url;
                    bool done = false;

                    //Youtube extractor - Get the infos on the video
                    IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
                    VideoInfo video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

                    if (video.RequiresDecryption)
                    {
                        DownloadUrlResolver.DecryptDownloadUrl(video);
                    }
                    var videoDownloader = new VideoDownloader(video, Path.Combine(plName, songName + video.VideoExtension));

                    //Try to download the video
                    try
                    {
                        videoDownloader.Execute();
                        done = true;

                    }
                    catch (Exception)
                    {
                    }

                    //If the video was downloaded succesfully
                    if (done)
                    {
                        //Set all the paths
                        string p1 = Path.Combine(baseDir, plName, songName + ".mp4");
                        string p2 = Path.Combine(baseDir, plName, songName + ".mp3");
                        var inputFile = new MediaFile { Filename = p1 };
                        var outputFile = new MediaFile { Filename = p2 };

                        using (var engine = new Engine())
                        {
                            //Convert the video to mp3 then deletes it
                            engine.Convert(inputFile, outputFile);
                        }
                        System.IO.File.Delete(p1);

                        //Set the mp3 tags for artists and title
                        TagLib.File file = TagLib.File.Create(p2);
                        file.Tag.Performers = ((IEnumerable<SimpleArtist>)song.Track.Artists).Select(p => p.Name).ToArray();
                        file.Tag.Title = song.Track.Name;
                        file.Tag.Album = song.Track.Album.Name;
                        file.Save();


                        Console.WriteLine(string.Format("{0} : downloaded. {1}/{2}", songName, c.ToString(), playlistTracks.Count.ToString()));
                        c++;
                        break;
                    }
                }
            }
            Console.WriteLine("Done! You can find your songs in the \"" + plName + "\" folder in the root of the application.");
            Process.Start(Path.Combine(baseDir, plName));
            Console.Read();
        }

        /// <summary>
        /// Replace windows forbidden characters
        /// </summary>
        /// <param name="input">File name</param>
        /// <returns>File name cleaned</returns>
        private static string ClearChars(string input)
        {
            string tmp = input;
            tmp = tmp.Replace('\"', ' ');
            tmp = tmp.Replace('[', ' ');
            tmp = tmp.Replace(']', ' ');
            tmp = tmp.Replace('{', ' ');
            tmp = tmp.Replace('}', ' ');
            tmp = tmp.Replace('\'', ' ');
            tmp = tmp.Replace(',', ' ');
            tmp = tmp.Replace('.', ' ');
            return tmp;
        }
    }
}
