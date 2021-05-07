using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KetwarooPortiaModCrafting
{
    class FactorySharedContainerBase
    {

        public Pathea.FarmFactoryNs.FarmFactory factory;
        public int factoryId;
        public FactorySharedContainerBase(int fid)
        {
           factoryId = fid;
        }

        public Pathea.FarmFactoryNs.FarmFactory getFactory()
        {

            if (factory == null)
            {
                factory = Pathea.FarmFactoryNs.FarmFactoryMgr.Self.GetFactory(factoryId);
            }
            return factory;
        }
    }
}
