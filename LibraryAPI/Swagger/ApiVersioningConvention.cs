using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace LibraryAPI.Swagger
{
    public class ApiVersioningConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Example: "Controllers.V1"
            var namespaceController = controller.ControllerType.Namespace;
            var version = namespaceController!.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}