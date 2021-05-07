using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KetwarooPortiaModCrafting
{
    class FactorySharedContainerMaterials : FactorySharedContainerBase, Pathea.HomeNs.IItemContainer
    {


        public FactorySharedContainerMaterials(int fid)
            : base(fid)
        {

        }

        public int Delete(int itemId, int count)
        {
            getFactory();
            if (factory != null)
            {
                // the Mat count in factory will be overriden.
                // use local one to avoid circular references.
                int matcount = this.GetCount(itemId);

                if (matcount == 0)
                {
                    //try next
                    return count;
                }

                int toremove = (matcount >= count) ? count : (count - matcount);
;
                factory.RemoveMat(itemId, toremove);

                return (count - toremove);

            }
            return count;
        }

        public int GetCount(int itemId)
        {
            getFactory();
            // prevent circular reference.
            if (factory != null)
            {
                foreach (IdCount idCount in factory.MatList)
                {
                    if (itemId == idCount.id)
                    {
                        return idCount.count;
                    }
                }
            }
            else
            {
                Main.dump("FactorySharedContainerMaterials FACTORY IS NULL");
            }
            return 0;
        }

    }

}
