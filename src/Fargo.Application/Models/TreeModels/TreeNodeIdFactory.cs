using System.Security.Cryptography;
using System.Text;

namespace Fargo.Application.Models.TreeModels;

public static class TreeNodeIdFactory
{
    public static Nodeid Create(
        TreeNodeType treeNodeType,
        Guid entityGuid,
        Nodeid? parentNodeId = null)
    {
        if (parentNodeId is null)
        {
            return new Nodeid(treeNodeType, entityGuid);
        }

        var input = $"{treeNodeType}:{entityGuid}:{parentNodeId.Value}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var guidBytes = hash[..16];

        return new Nodeid(treeNodeType, new Guid(guidBytes));
    }
}
