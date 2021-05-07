using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KetwarooPortiaModCrafting
{
    class FactorySharedContainerFinishedProduct : FactorySharedContainerBase, Pathea.HomeNs.IItemContainer
    {

        public FactorySharedContainerFinishedProduct(int fid)
            : base(fid)
        {

        }

        public int Delete(int itemId, int count)
        {
            getFactory();
            if (factory != null)
            {
                int matcount = this.GetCount(itemId);

                if (matcount == 0)
                {
                    //try next
                    return count;
                }

                int toremove = (matcount >= count) ? count : (count - matcount);

                factory.RemoveFinshedProduct(itemId, toremove);
                factory.FreshFinshed();
                return (count - toremove);

            }

            return count;
        }

        public int GetCount(int itemId)
        {

            getFactory();
           

            if (factory != null)
            {
                // should be fine.
                return factory.GetFinishedItemCount(itemId);
            }
 
            return 0;
        }


    }

}
