
using System.Collections.Generic;
using Filmster.Data;

namespace Filmster.Web.Models
{
    public class CatalogViewModel
    {
        public List<Movie> Movies { get; set; }
        public string SelectedValue { get; set; }
        public string[] Alpabet
        {
            get { return "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,Æ,Ø,Å".Split(','); }
        }
    }
}