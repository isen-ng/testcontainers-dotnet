using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Container.Test.Utility
{
    public static class DockerClientHelper
    {
        public static async Task DeleteImage(IDockerClient dockerClient, string imageName)
        {
            var images = await dockerClient.Images.ListImagesAsync(new ImagesListParameters());
            var existingImage = images.FirstOrDefault(i => i.RepoTags != null && i.RepoTags.Contains(imageName));
            if (existingImage != null)
            {
                var parameters = new ImageDeleteParameters {Force = true};

                await dockerClient.Images.DeleteImageAsync(imageName, parameters);
            }
        }

        public static async Task DeleteNetwork(IDockerClient dockerClient, string networkName)
        {
            var networks = await dockerClient.Networks.ListNetworksAsync(new NetworksListParameters());
            var existingNetwork = networks.FirstOrDefault(i => string.Equals(i.Name, networkName));
            if (existingNetwork != null)
            {
                await dockerClient.Networks.DeleteNetworkAsync(existingNetwork.ID);
            }
        }
    }
}
