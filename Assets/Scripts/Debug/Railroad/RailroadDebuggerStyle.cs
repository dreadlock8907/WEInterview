using UnityEngine;

namespace WE.Debug.Railroad
{
    public static class RailroadDebuggerStyle
    {
        public static class UI
        {
            public static readonly Color SelectedBackground = new Color(0, 0.4f, 0, 1);
        }

        public static class Node
        {
            public static readonly Color DefaultColor = Color.white;
            public static readonly Color SelectedColor = Color.green;
            public static readonly Color MineColor = new Color(0.8f, 0.4f, 0.0f);    // Оранжевый
            public static readonly Color BaseColor = new Color(0.0f, 0.6f, 1.0f);    // Голубой
            public const float Size = 0.5f;
            public const float SelectedSize = 0.7f;
        }

        public static class Connection
        {
            public static readonly Color DefaultColor = Color.white;
            public static readonly Color SelectedColor = Color.green;
            public const float Width = 2f;
        }

        public static class Labels
        {
            public static readonly Color TextColor = Color.white;
            public const int FontSize = 12;
            public static readonly TextAnchor Alignment = TextAnchor.MiddleCenter;
        }
    }
} 