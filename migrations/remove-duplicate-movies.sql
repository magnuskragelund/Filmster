-- Remove duplicate movies, keeping the one with the lowest Id.
-- Uncomment lines to first get rid of rental options, then get rid of movies
-- http://stackoverflow.com/questions/18932/sql-how-can-i-remove-duplicate-rows

--DELETE FROM RentalOptions where Movie_Id IN (SELECT Movies.Id
DELETE Movies
FROM Movies
LEFT OUTER JOIN (
   SELECT MIN(Id) as Id, Title, ReleaseDate
   FROM Movies 
   GROUP BY Title, ReleaseDate
) as KeepRows ON
   Movies.Id = KeepRows.Id
WHERE
   KeepRows.Id IS NULL
--)