using System;

namespace CDP.Core
{
    /// <summary>
    /// Maps all dependency interfaces to their implementations.
    /// </summary>
    public static class ObjectMappings
    {
        public static void Initialise()
        {
            ObjectCreator.Map<IDemoManager, DemoManager>();
            ObjectCreator.Map<IFileSystem, FileSystem>();
            ObjectCreator.Map<IProcess, Process>();
            ObjectCreator.Map<IProcessFinder, ProcessFinder>();
            ObjectCreator.Map<ISettings, Settings>();
            ObjectCreator.Map<IFlowDocumentWriter, FlowDocumentWriter>();
        }
    }
}
