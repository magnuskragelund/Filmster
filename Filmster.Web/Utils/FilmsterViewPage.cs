using System.Web.Mvc;
using Filmster.Data;

public abstract class FilmsterViewPage<T> : WebViewPage<T>
{
    private IFilmsterRepository _repo;
    public FilmsterViewPage()
    {
        _repo = new FilmsterRepository();
    }

    protected override void InitializePage()
    {
        SetViewBagDefaultProperties();
        base.InitializePage();
    }

    private void SetViewBagDefaultProperties()
    {
        ViewBag.Top5Movies = _repo.GetPopularMovies(5);
    }
}