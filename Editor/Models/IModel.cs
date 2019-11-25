namespace Unity.Cloud.Collaborate.Models
{
    internal interface IModel
    {
        /// <summary>
        /// Called when the model should be stopped and data should be saved or closed.
        /// </summary>
        void OnStop();
    }
}
