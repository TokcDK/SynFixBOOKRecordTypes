using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System.Threading.Tasks;

namespace SynFixBOOKRecordTypes
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "FixBOOKRecordTypesPatch.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var linkCache = state.LinkCache;
            foreach (var bookGetter in state.LoadOrder.PriorityOrder.Book().WinningOverrides())
            {
                var bookArt = bookGetter.InventoryArt;
                if (bookArt == null || bookArt.IsNull)
                {
                    // ignore if bookart is null
                    continue;
                }

                // try to get reference
                if (!bookArt.TryResolve(linkCache, out var art)) continue;

                var bookArtString = art.EditorID + "";
                var bookType = bookGetter.Type;

                // check art Static record edid and data type consistency
                bool isBook = (bookArtString.Contains("Book") || bookArtString.Contains("Journal"));
                if (isBook && bookType == Book.BookType.NoteOrScroll)
                {
                    // book or journal record has data type of note
                    // isBook = true;
                }
                else if (!isBook && bookArtString.Contains("Note") && bookType == Book.BookType.BookOrTome)
                {
                    // note record has data type of book
                    isBook = false;
                }
                else
                {
                    continue;
                }

                // set new data type value depending on art edid
                state.PatchMod.Books.GetOrAddAsOverride(bookGetter).Type = isBook ? Book.BookType.BookOrTome : Book.BookType.NoteOrScroll;

            }
        }
    }
}
