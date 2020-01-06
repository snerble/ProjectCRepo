package com.example.alldone;


import android.content.Intent;
import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

/**
 * A simple {@link Fragment} subclass.
 */
public class DescriptionTab extends Fragment {

    public DescriptionTab() {
        // Required empty public constructor
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle state) {

        View rootView = inflater.inflate(R.layout.description_tab, container, false);

        final Bundle args = this.getArguments();
        final String description = args.getString("Description");

        TextView descriptionText = rootView.findViewById(R.id.descriptionTxt);
        descriptionText.setText(description);

        // Inflate the layout for this fragment
        return rootView;
    }
}
