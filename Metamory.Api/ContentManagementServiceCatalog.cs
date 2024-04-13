using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Metamory.Api.Repositories;

namespace Metamory.Api


class ContentManagementServiceCatalog
{
    public ContentManagementService Get(string name)
    {
        return new ContentManagementService();
    }
}