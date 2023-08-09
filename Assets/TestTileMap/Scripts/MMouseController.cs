using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MMouseController : MonoBehaviour
{
    public float speed;
    public GameObject characterPrefab;
    public MCharacterInfo character;

    private MPathFinder pathFinder;
    private List<MOverlayTile> path = new List<MOverlayTile>();

    private void Start()
    {
        pathFinder = new MPathFinder();
    }

    private void LateUpdate()
    {
        var focusedTileHit = GetFocusedOnTile();

        if (focusedTileHit.HasValue)
        {
            MOverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<MOverlayTile>();
            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder;

            if (Input.GetMouseButtonDown(0))
            {
                overlayTile.GetComponent<MOverlayTile>().ShowTile();
                if (character == null)
                {
                    character = Instantiate(characterPrefab).GetComponent<MCharacterInfo>();
                    PositionCharacterOnTile(overlayTile);
                }
                else
                {
                    path = pathFinder.FindPath(character.activeTile, overlayTile);
                }
            }
        }

        if (path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        var step = speed + Time.deltaTime;
        var zIndex = path[0].transform.position.z;
        character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, step);
        character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);
        if (Vector2.Distance(character.transform.position, path[0].transform.position) < 0.0001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }
    }
    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2d, Vector2.zero);
        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    private void PositionCharacterOnTile(MOverlayTile _overlayTile)
    {
        character.transform.position = new Vector3(_overlayTile.transform.position.x, _overlayTile.transform.position.y + 0.001f, _overlayTile.transform.position.z);
        character.GetComponent<SpriteRenderer>().sortingOrder = _overlayTile.GetComponent<SpriteRenderer>().sortingOrder;
        character.activeTile = _overlayTile;
    }
}
