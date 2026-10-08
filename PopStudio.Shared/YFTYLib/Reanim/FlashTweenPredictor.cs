using System.Globalization;

namespace PopStudio.Reanim
{
    // Reconstructs only linear classic tweens. Every omitted sample must pass
    // all channel tolerances; unsupported transforms remain explicit keyframes.
    internal static class FlashTweenPredictor
    {
        // Exported x/y (pixels), kx/ky (degrees), sx/sy (ratios), alpha (0..1).
        private static readonly double[] Tolerances = { 0.05, 0.05, 0.05, 0.05, 0.0005, 0.0005, 0.005 };
        private const double NumericalSlack = 0.000001;

        internal readonly record struct Span(int Duration, bool Tween);

        public static bool[] GetBoundaries(Reanim animation)
        {
            bool[] boundaries = new bool[animation.tracks.Max(track => track.transforms.Length)];
            foreach (ReanimTrack track in animation.tracks)
            {
                float frame = 0;
                for (int i = 0; i < track.transforms.Length; i++)
                {
                    float next = track.transforms[i].f ?? frame;
                    // Marker tracks delimit actions for every layer, including
                    // layers whose own image and visibility do not change.
                    if (next != frame) boundaries[i] = true;
                    frame = next;
                }
            }
            return boundaries;
        }

        public static Span[] Predict(ReanimTrack track, bool[] boundaries, double scaleX, double scaleY)
        {
            Sample[] samples = Resolve(track, scaleX, scaleY);
            Span[] spans = Enumerable.Repeat(new Span(1, false), samples.Length).ToArray();
            int start = 0;
            while (start < samples.Length)
            {
                int end = start;
                while (end + 1 < samples.Length && !boundaries[end + 1]
                    && samples[start].SameContent(samples[end + 1])) end++;
                Fit(samples, spans, start, end);
                start = end + 1;
            }

            // Join identical holds, including the final endpoint, without
            // removing a tween's destination or crossing an action boundary.
            for (int i = 0; i < spans.Length;)
            {
                int next = i + spans[i].Duration;
                while (!spans[i].Tween && next < spans.Length && !boundaries[next]
                    && !spans[next].Tween && samples[i].SameContent(samples[next])
                    && samples[i].SameValues(samples[next]))
                {
                    spans[i] = new Span(spans[i].Duration + spans[next].Duration, false);
                    spans[next] = default;
                    next = i + spans[i].Duration;
                }
                i = next;
            }
            return spans;
        }

        private static void Fit(Sample[] samples, Span[] spans, int start, int end)
        {
            if (end - start < 2) return;
            // Bound both fitting cost and recursion depth on long animations.
            int split = start + (end - start) / 2;
            if (end - start <= 256)
            {
                Sample first = samples[start];
                Sample last = samples[end];
                bool constant = true;
                bool fixedTransform = true;
                bool rigid = true;
                for (int i = start; i <= end; i++)
                {
                    constant &= first.SameValues(samples[i]);
                    fixedTransform &= first.SameTransform(samples[i]);
                    rigid &= samples[i].IsRigid();
                }
                // Changing skew, mirrored/degenerate transforms and ambiguous
                // half/full turns are not inferred in this conservative mode.
                bool supported = first.Visible && (fixedTransform || rigid)
                    && Math.Abs(last.Values[2] - first.Values[2]) < 170
                    && Math.Abs(last.Values[3] - first.Values[3]) < 170;
                double worst = 0;
                if (!constant && supported)
                {
                    for (int i = start + 1; i < end; i++)
                    {
                        double progress = (double)(i - start) / (end - start);
                        for (int channel = 0; channel < Tolerances.Length; channel++)
                        {
                            double predicted = first.Values[channel]
                                + (last.Values[channel] - first.Values[channel]) * progress;
                            double error = Math.Abs(predicted - samples[i].Values[channel])
                                / (Tolerances[channel] + NumericalSlack);
                            if (!double.IsFinite(error)) error = double.PositiveInfinity;
                            if (error > worst)
                            {
                                worst = error;
                                split = i;
                            }
                        }
                    }
                }
                if (constant || (supported && worst <= 1))
                {
                    spans[start] = new Span(end - start, !constant);
                    for (int i = start + 1; i < end; i++) spans[i] = default;
                    return;
                }
            }
            Fit(samples, spans, start, split);
            Fit(samples, spans, split, end);
        }

        private static Sample[] Resolve(ReanimTrack track, double scaleX, double scaleY)
        {
            Sample[] samples = new Sample[track.transforms.Length];
            float x = 0, y = 0, kx = 0, ky = 0, sx = 1, sy = 1, alpha = 1, frame = 0;
            string image = null, text = null, font = null, image2 = null, imagePath = null, image2Path = null;
            for (int i = 0; i < samples.Length; i++)
            {
                ReanimTransform transform = track.transforms[i];
                x = transform.x ?? x;
                y = transform.y ?? y;
                kx = transform.kx ?? kx;
                ky = transform.ky ?? ky;
                sx = transform.sx ?? sx;
                sy = transform.sy ?? sy;
                alpha = transform.a ?? alpha;
                frame = transform.f ?? frame;
                if (transform.i != null) image = Convert.ToString(transform.i, CultureInfo.InvariantCulture);
                text = transform.text ?? text;
                font = transform.font ?? font;
                image2 = transform.i2 ?? image2;
                imagePath = transform.iPath ?? imagePath;
                image2Path = transform.i2Path ?? image2Path;
                samples[i] = new Sample
                {
                    Values = new double[] { x * scaleX, y * scaleY, kx, ky, sx, sy, alpha },
                    Frame = frame,
                    Content = new[] { image, text, font, image2, imagePath, image2Path }
                };
            }
            return samples;
        }

        private sealed class Sample
        {
            public double[] Values;
            public float Frame;
            public string[] Content;
            public bool Visible => Frame != -1;

            public bool SameContent(Sample other) => Frame == other.Frame && Content.SequenceEqual(other.Content);
            public bool SameValues(Sample other) => Values.Zip(other.Values).All(pair => pair.First == pair.Second);
            public bool SameTransform(Sample other) => Enumerable.Range(2, 4).All(i => Values[i] == other.Values[i]);
            public bool IsRigid() => Values.All(double.IsFinite) && Values[4] > 0.000001 && Values[5] > 0.000001
                && Math.Abs(Math.IEEERemainder(Values[2] - Values[3], 360)) < NumericalSlack;
        }
    }
}
