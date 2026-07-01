namespace Core.camera
{
    /*
     * Holds the data where we come from and which cameras paths we should take to get to the new spot
     */
    [System.Serializable]
    public class CameraRoute
    {
        public string fromSpotId;
        public string[] wayCamerasIds;
    }
}
