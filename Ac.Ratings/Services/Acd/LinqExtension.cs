namespace Ac.Ratings.Services.Acd;

public static class LinqExtension {
    public static IEnumerable<T> NonNull<T>(this IEnumerable<T> source) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Where(i => !Equals(i, default(T)));
    }

    public static IEnumerable<T> ApartFrom<T>(this IEnumerable<T> source, object item) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Where(x => !ReferenceEquals(x, item));
    }

    public static IEnumerable<T> ApartFrom<T>(this IEnumerable<T> source, T item) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Where(x => !Equals(x, item));
    }

    public static IEnumerable<T> ApartFrom<T>(this IEnumerable<T> source, params T[] additionalItems) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (additionalItems == null) return source;
        return source.Where(x => !additionalItems.ArrayContains(x));
    }

    public static bool ArrayContains<T>(this T[] array, T value) {
        return Array.IndexOf(array, value) != -1;
    }
}