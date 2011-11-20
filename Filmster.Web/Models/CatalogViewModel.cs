using System.Collections.Generic;
using Filmster.Data;

namespace Filmster.Web.Models
{
    public class CatalogViewModel
    {
        public List<Movie> Movies { get; set; }
        public string SelectedValue { get; set; }
        public string[] Alphabet
        {
            get { return "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,æ,ø,å".Split(','); }
        }
    }
}