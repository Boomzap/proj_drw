using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ho
{
    public interface IHOReactor
    {
        // item found, do animation and set next item
        void    UpdateActiveItemInList(HOFindableObject foundObject, IEnumerable<HOFindableObject> nextObjects);
        void    OnItemListEmpty();
        void    SetInitialItemList(List<HOFindableObject> initialObjects, int totalToFind);
    }
}
