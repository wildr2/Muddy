using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool debug_elf_eyes = false;
    public string initial_path;

    public GameObject landmarks_obj;
    public Text narration_text;
    public float speed = 3.0f;
    public AnimationCurve distance_curve;
    public float max_ui_pos = 800.0f;
    public float max_dist = 100.0f;
    public Color highlight_color = Color.red;

    public float player_position;
    public float target_player_position;
    private float prev_input_move;
    private bool teleporting;
    private Path player_path;
    private Path prev_player_path;
    private Landmark inspected_lm;
    private float path_timestamp = -999.0f;
    private Landmark from_door;
    private Searcher<Landmark> landmark_searcher;
    private float step_time;

    private void Start()
    {
        landmark_searcher = new Searcher<Landmark>(Landmark.Filter);

        Path[] paths = FindObjectsOfType<Path>();
        player_path = Array.Find(paths, (path) => path.name == initial_path);
        player_path = player_path ? player_path : paths[0];

        SetupLandmarks();
    }

    private void SetupLandmarks()
    {
        Landmark[] landmarks = FindObjectsOfType<Landmark>();
        for (int i = 0; i < landmarks.Length; ++i)
        {
            Landmark lm = landmarks[i];
            if (lm.door_name != "")
            {
                for (int j = i + 1; j < landmarks.Length; ++j)
                {
                    Landmark lm2 = landmarks[j];
                    if (lm.door_name == lm2.door_name)
                    {
                        lm.other_door = lm2;
                        lm2.other_door = lm;
                    }
                }
            }
        }
    }

    private void Update()
    {
        // Interaction.
        bool input_interact = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Return);
        bool interacted = false;
        if (input_interact && inspected_lm)
        {
            if (inspected_lm.other_door)
            {
                prev_player_path = player_path;
                player_path = inspected_lm.other_door.GetPath();
                //player_position = inspected_lm.other_door.position + (player_position - inspected_lm.position);
                //target_player_position = player_position;
                player_position = inspected_lm.other_door.position;
                target_player_position = inspected_lm.other_door.position;
                //teleporting = true;
                path_timestamp = Time.timeSinceLevelLoad;
                inspected_lm.other_door.index = inspected_lm.index;
                from_door = inspected_lm;
                RectTransform from_door_rt = from_door.GetComponent<RectTransform>();
                RectTransform to_door_rt = from_door.other_door.GetComponent<RectTransform>();
                to_door_rt.localPosition = from_door_rt.localPosition;
                inspected_lm = null;
            }
            else
            {
                target_player_position = inspected_lm.position;
                //player_position = target_player_position;
                inspected_lm = null;
            }
            landmark_searcher.Reset();
            interacted = true;
        }

        // Landmarks.
        Landmark[] landmarks = FindObjectsOfType<Landmark>();
        //Array.Sort(landmarks, new ReverseComparer(new CompareLandmarkEdgeofVisionPos(player_position, player_path, false)));
        Array.Sort(landmarks, new ReverseComparer(new CompareLandmarkName()));

        Landmark[] visible_landmarks = landmarks.Where(lm => lm.GetPath() == player_path && Mathf.Abs(lm.position - player_position) < lm.los).ToArray();

        Landmark[] visible_landmarks_by_vis = (Landmark[])visible_landmarks.Clone();
        Array.Sort(visible_landmarks_by_vis, new CompareLandmarkVisibility(player_position, player_path));

        Landmark[] visible_landmarks_by_index = (Landmark[])visible_landmarks.Clone();
        Array.Sort(visible_landmarks_by_index, new CompareLandmarkIndex());

        Path path = null;
        int path_first_index = -1;
        for (int i = 0; i < landmarks.Length; ++i)
        {
            Landmark landmark = landmarks[i];
            RectTransform rt = landmark.GetComponent<RectTransform>();
            Text text = landmark.GetComponent<Text>();

            Path lm_path = landmark.transform.parent.GetComponent<Path>();
            if (path != lm_path)
            {
                path = lm_path;
                path_first_index = i;
            }
            int path_row = i - path_first_index;

            bool on_path = lm_path == player_path;
            bool on_prev_path = from_door && lm_path == from_door.GetPath();
            bool is_from_door = landmark == from_door;
            bool is_to_door = from_door && landmark == from_door.other_door;
            float dif = landmark.position - player_position;
            float dist = Mathf.Abs(landmarks[i].position - player_position);
            bool is_visible = dist < landmarks[i].los && on_path;

            landmark.transform.name = landmark.name;
            text.text = landmark.name;
            if (is_to_door)
            {
                float t = (Time.timeSinceLevelLoad - path_timestamp - 0.5f) * 6.0f;
                landmark.SetText(from_door.name, landmark.name, t);
            }

            bool is_inspected = landmark == inspected_lm;
            if (is_inspected && !is_visible)
            {
                is_inspected = false;
                inspected_lm = null;
                landmark_searcher.Reset();
            }
            landmark.inspect_indicator.gameObject.SetActive(is_inspected);

            int old_lm_index = landmark.index;
            landmark.index = !is_visible ? -1 : landmark.index < 0 ? get_next_available_landmark_index(visible_landmarks_by_index) : landmark.index;
            if (landmark.index != old_lm_index)
            {
                Array.Sort(visible_landmarks_by_index, new CompareLandmarkIndex());
            }

            Vector3 target_ui_pos = new Vector3();
            int row = path_row;
            target_ui_pos.x = distance_curve.Evaluate(Mathf.Clamp01(dist / max_dist)) * Mathf.Sign(dif) * max_ui_pos;
            target_ui_pos.y = (landmarks.Length - 1 - landmark.index) * 35.0f;
             
            Vector3 ui_pos = rt.localPosition;
            if (on_path)
            {
                float x_lerp_speed = 4; // 10
                float target_x = teleporting ? target_ui_pos.x : Mathf.Lerp(ui_pos.x, target_ui_pos.x, Time.deltaTime * x_lerp_speed);
                //float to_target_x = target_x - ui_pos.x;
                //ui_pos.x = ui_pos.x + Mathf.Sign(to_target_x) * Mathf.Min(Mathf.Abs(to_target_x), 1000 * Time.deltaTime);
                ui_pos.x = target_x;
                ui_pos.y = on_path ? target_ui_pos.y : ui_pos.y; // Mathf.Lerp(ui_pos.y, target_ui_pos.y, Time.deltaTime * landmark.y_lerp_speed);
                rt.localPosition = ui_pos;
            }

            Color color = text.color;
            if (on_path)
            {
                float on_path_opacity = landmark.los == 0 ? 0.0f : Mathf.Clamp01(1.0f - Mathf.Pow(dist / landmark.los, 2.0f));
                float t = is_to_door ?
                    1.0f : // (Time.timeSinceLevelLoad - path_timestamp) * 2.0f:
                    (Time.timeSinceLevelLoad - path_timestamp - 0.5f) * 2.0f;
                color.a = Mathf.Lerp(on_path_opacity - 1.0f, on_path_opacity, t);
                if (debug_elf_eyes)
                {
                    color.a = 1.0f;
                }
                landmark.last_on_path_opacity = color.a;
            }
            else if (on_prev_path)
            {
                float t = is_from_door ?
                    1.0f : // (Time.timeSinceLevelLoad - path_timestamp) * 2.0f :
                    (Time.timeSinceLevelLoad - path_timestamp) * 2.0f;
                color.a = Mathf.Lerp(landmark.last_on_path_opacity, landmark.last_on_path_opacity - 1.0f, t);
            }
            else
            {
                color.a = 0.0f;
            }
            text.color = color;
        }

        // Movement.
        //bool input_left = Input.GetKey(KeyCode.Comma);
        //bool input_right = Input.GetKey(KeyCode.Period);
        bool input_left = Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.LeftShift);
        bool input_right = Input.GetKey(KeyCode.L) && !Input.GetKey(KeyCode.LeftShift);
        int input_move = (input_left ? -1 : 0) + (input_right ? 1 : 0);
        int move_dir = Mathf.Abs(target_player_position - player_position) > 0.1f ? (int)Mathf.Sign(target_player_position - player_position) : 0;

        bool input_step_left = Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.LeftShift);
        bool input_step_right = Input.GetKey(KeyCode.L) && !Input.GetKey(KeyCode.LeftShift);
        int input_step = (input_step_left ? -1 : 0) + (input_step_right ? 1 : 0);
        input_step = 0;

        if (input_step != 0 && Time.time - step_time > 0.25f)
        {
            Landmark[] path_landmarks_by_dist = landmarks.Where(lm => lm.GetPath() == player_path).ToArray();
            Array.Sort(path_landmarks_by_dist, new CompareLandmarkDist(player_position));
            if (path_landmarks_by_dist.Length > 0)
            {
                Landmark closest_path_landmark = path_landmarks_by_dist[0];
                Landmark[] path_landmarks_by_pos = (Landmark[])path_landmarks_by_dist.Clone();
                Array.Sort(path_landmarks_by_pos, new CompareLandmarkPosition());

                int closest_i = Array.FindIndex(path_landmarks_by_pos, (lm) => lm == closest_path_landmark);
                Landmark target =
                    input_step < 0 && closest_i > 0 ? path_landmarks_by_pos[closest_i - 1] :
                    input_step > 0 && closest_i < path_landmarks_by_pos.Length - 1 ? path_landmarks_by_pos[closest_i + 1] : null;
                if (target)
                {
                    target_player_position = target.position;
                    //player_position = target_player_position; // maybe
                    inspected_lm = target;
                    step_time = Time.time;
                }
            }
        }

        if (input_move != 0)
        {
            target_player_position = player_position + input_move * 50;
        }
        else if (prev_input_move != 0)
        {
            target_player_position = player_position;
        }
        prev_input_move = input_move;
        player_position += Mathf.Clamp(target_player_position - player_position, -speed * Time.deltaTime, speed * Time.deltaTime);

        player_position = Mathf.Clamp(player_position, player_path.left_wall_pos, player_path.right_wall_pos);


        // Search.
        string input_search = Regex.Replace(Input.inputString, "[^A-Za-z0-9 -]", "").ToLower();
        if (input_search.Length > 0 && (input_move == 0 || (input_search != "j" && input_search != "l")))
        {
            Landmark old_inspected_lm = inspected_lm;
            landmark_searcher.Update(input_search, visible_landmarks_by_vis);
            inspected_lm = landmark_searcher.IsFound() ? landmark_searcher.last_found_item : null;
            if (inspected_lm && inspected_lm != old_inspected_lm)
            {
                //target_player_position = inspected_lm.position;
                //player_position = target_player_position; // maybe
            }
        }

        // Selection.
        bool input_inspect = Input.GetKeyDown(KeyCode.Slash);
        bool input_continue = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Return);
        bool input_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (input_inspect && visible_landmarks.Length > 0)
        {
            int index = Array.FindIndex(visible_landmarks_by_index, x => x == inspected_lm);
            int dir = input_shift ? -1 : 1;
            inspected_lm = index < 0 ? visible_landmarks_by_vis[0] : visible_landmarks_by_index[Mod(index + dir, visible_landmarks_by_index.Length)];
            landmark_searcher.Reset();
        }
        else if (input_continue && !interacted)
        {
            inspected_lm = null;
            target_player_position = player_position;
        }

        // Narration.
        float path_out_t = (Time.timeSinceLevelLoad - path_timestamp) * 2.0f > 0.0f ? 1.0f : 0.0f;
        float path_in_t = (Time.timeSinceLevelLoad - path_timestamp) * 2.0f - 2.0f;
        float narration_opacity = Mathf.Lerp(0.0f, 0.0f, path_out_t) + Mathf.Lerp(0.0f, 1.0f, path_in_t);
        if (inspected_lm)
        {
            narration_text.text = inspected_lm.GetDesc(player_position) + " >";
            narration_text.color = Color.Lerp(Color.clear, Color.white, 0.75f * narration_opacity);
        }
        else
        {
            narration_text.text = path_in_t > 0.0f ? player_path.description : prev_player_path ? prev_player_path.description : "";
            narration_text.color = Color.Lerp(Color.clear, Color.white, 0.75f * narration_opacity);
        }

        if (teleporting)
        {
            teleporting = false;
            player_position = from_door.other_door.position;
            target_player_position = player_position;
        }
    }

    public int get_next_available_landmark_index(Landmark[] visible_landmarks_by_index)
    {
        int index = 0;
        for (int i = 0; i < visible_landmarks_by_index.Length; ++i)
        {
            if (index == visible_landmarks_by_index[i].index)
            {
                ++index;
            }
            if (index < visible_landmarks_by_index[i].index)
            {
                break;
            }
        }
        return index;
    }

    public static T[] ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            int r = UnityEngine.Random.Range(0, array.Length - 1);
            T temp = array[r];
            array[r] = array[i];
            array[i] = temp;
        }
        return array;
    }

    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}

public class Searcher<T> where T : class
{
    public const float max_input_delay = 0.3f;
    private string search_str;
    private Func<T, string, bool> filter;
    public float start_time;
    public float input_time;
    public float found_time;
    public T last_found_item;
    private HashSet<T> visited = new HashSet<T>();

    public Searcher(Func<T, string, bool> filter)
    {
        this.filter = filter;
        Reset();
    }

    public bool IsSearching()
    {
        return Time.time - input_time <= max_input_delay;
    }

    public bool IsFound()
    {
        return found_time > 0 && found_time >= start_time;
    }

    public void Update(string input_str, IEnumerable<T> items)
    {
        if (input_str == "")
        {
            return;
        }

        float time_since_input = Time.time - input_time;
        if (time_since_input > max_input_delay)
        {
            if (search_str != input_str)
            {
                // Search t for "oak tree", then t again for "tree", then o for "oak tree", should not consider oak tree already visited.
                visited.Clear();
            }

            search_str = "";
            start_time = Time.time;
        }

        search_str += input_str;
        bool extended_search = input_str != search_str;

        IEnumerable<T> matches = items.Where(item => filter(item, search_str));
        T[] matches_array = matches.ToArray();

        // Search t for "oak tree", then o, should not revisit "oak tree".
        if (matches_array.Length > 0 && matches_array[0] == last_found_item)
        {
            visited.Add(last_found_item);
        }

        bool visited_all = false;
        bool recycling = false;
        if (!extended_search && matches_array.Length > 0)
        {
            T[] unvisited_matches_array = matches.Where(item => !visited.Contains(item)).ToArray();
            visited_all = unvisited_matches_array.Length == 0;

            if (visited_all && matches_array.Length > 1)
            {
                // Cycle again.
                recycling = true;
                visited.Clear();
            }
            else
            {
                matches_array = unvisited_matches_array;
            }
        }

        if (visited_all && !recycling)
        {
            // Toggle.
            Reset();
        }
        else if (matches_array.Length > 0)
        {
            last_found_item = matches_array[0];
            visited.Add(last_found_item);
            found_time = Time.time;
        }
        else if (extended_search)
        {
            search_str = "";
            start_time = Time.time;
            Update(input_str, items);
        }
        else
        {
            Reset();
        }

        input_time = Time.time;
    }

    public void Reset()
    {
        last_found_item = null;
        visited.Clear();
        start_time = -1;
        input_time = -1;
        found_time = -1;
        search_str = "";
    }
}
