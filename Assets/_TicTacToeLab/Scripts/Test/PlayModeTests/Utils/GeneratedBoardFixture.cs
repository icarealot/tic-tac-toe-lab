#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Generates the board adapter's cell and mark templates so isolated checks never load production assets.
    /// The board template is generated on request for callers that compose the adapter, such as Bootstrap's ownership checks.
    /// </summary>
    public sealed class GeneratedBoardFixture
    {
        private const int BOARD_DIMENSION = 3;

        public BoardView BoardTemplate
        {
            get
            {
                if (_boardTemplate == null)
                {
                    GameObject boardRoot = CreateRoot("Generated board template");
                    _boardTemplate = boardRoot.AddComponent<BoardView>();
                    TestSerializedReference.AssignPrefab(_boardTemplate, "_cellViewPrefab", _cellTemplate);
                }

                return _boardTemplate;
            }
        }

        public BoardLayout Layout { get; }

        private readonly List<GameObject> _generatedRoots = new();
        private readonly CellView _cellTemplate;
        private BoardView _boardTemplate;

        public GeneratedBoardFixture()
        {
            GameObject markRoot = CreateRoot("Generated mark template");
            MarkView markTemplate = markRoot.AddComponent<MarkView>();

            GameObject cellRoot = CreateRoot("Generated cell template");
            _cellTemplate = cellRoot.AddComponent<CellView>();
            TestSerializedReference.AssignPrefab(_cellTemplate, "_markViewPrefab", markTemplate);

            Layout = new BoardLayout(BOARD_DIMENSION);
        }

        public BoardView CreateBoard()
        {
            BoardLayout layout = Layout;
            IReadOnlyList<CellPlacement> placements = layout.GetCellPlacements();
            BoardView board = Object.Instantiate(BoardTemplate);
            _generatedRoots.Add(board.gameObject);
            board.Construct(BOARD_DIMENSION, placements);
            return board;
        }

        public CellView CreateCell(CellCoordinate coordinate)
        {
            BoardLayout layout = Layout;
            Vector3 localPoint = layout.GetCellLocalPoint(coordinate);
            CellView cell = Object.Instantiate(_cellTemplate);
            _generatedRoots.Add(cell.gameObject);
            cell.Construct(new CellPlacement(coordinate, localPoint));
            return cell;
        }

        public CellView FindCell(BoardView board, CellCoordinate coordinate)
        {
            BoardLayout layout = Layout;
            Vector3 expectedLocalPoint = layout.GetCellLocalPoint(coordinate);

            CellView matchingCell = board.GetComponentsInChildren<CellView>()
                .FirstOrDefault(cell => cell.transform.localPosition == expectedLocalPoint);

            Assert.That(
                matchingCell,
                Is.Not.Null,
                $"The board should create a cell at cell coordinate ({coordinate.Row}, {coordinate.Column}).");

            return matchingCell;
        }

        public IEnumerator IE_DestroyAll()
        {
            GameObject[] generatedRoots = _generatedRoots.ToArray();
            _generatedRoots.Clear();

            foreach (GameObject generatedRoot in generatedRoots)
            {
                if (generatedRoot != null)
                {
                    Object.Destroy(generatedRoot);
                }
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => generatedRoots.All(root => root == null),
                "The generated board objects should be destroyed after the test.");
        }

        private GameObject CreateRoot(string name)
        {
            GameObject root = new(name);
            _generatedRoots.Add(root);
            return root;
        }
    }
}
#endif
