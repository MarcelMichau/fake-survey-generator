import React, { useState } from 'react';

const GetSurvey = ({ surveyId, onUpdateSurveyId, onFetch, surveyDetail }) => {
	return (
		<div>
			<label>
				Survey ID
				<div>
					<input
						type="text"
						value={surveyId}
						onChange={e => onUpdateSurveyId(e.target.value)}
					/>
				</div>
			</label>

			<button onClick={onFetch}>Fetch</button>

			{surveyDetail && (
				<div
					style={{ border: '2px solid white', margin: '1em', padding: '1em' }}
				>
					<h3>
						This Survey Asked{' '}
						<em>
							{surveyDetail.numberOfRespondents} {surveyDetail.respondentType}
						</em>{' '}
						the Question:
					</h3>
					<h2>
						<em>{surveyDetail.topic}</em>
					</h2>
					<h3>And the results were:</h3>

					<ul>
						{surveyDetail.options.map((option, index) => (
							<li key={index}>
								{option.optionText} - {option.numberOfVotes} votes
							</li>
						))}
					</ul>
				</div>
			)}
		</div>
	);
};

const CreateSurvey = ({ onCreateSurvey }) => {
	const [respondentType, setRespondentType] = useState('');
	const [topic, setTopic] = useState('');
	const [numberOfRespondents, setNumberOfRespondents] = useState(0);
	const [options, setOptions] = useState([{ id: 1, optionText: '' }]);

	const updateOption = (optionId, optionText) => {
		setOptions(
			options.map(option => {
				if (option.id !== optionId) return option;

				return {
					...option,
					optionText
				};
			})
		);
	};

	const onSubmit = e => {
		e.preventDefault();
		const surveyCommand = {
			surveyTopic: topic,
			numberOfRespondents,
			respondentType,
			surveyOptions: options.map(option => ({ optionText: option.optionText }))
		};

		onCreateSurvey(surveyCommand);

		setRespondentType('');
		setTopic('');
		setNumberOfRespondents(0);
		setOptions([{ id: 1, optionText: '' }]);
	};

	return (
		<div>
			<form onSubmit={onSubmit}>
				<label>
					Target Audience
					<div>
						<input
							type="text"
							value={respondentType}
							onChange={e => setRespondentType(e.target.value)}
						/>
					</div>
				</label>
				<label>
					Question
					<div>
						<input
							type="text"
							value={topic}
							onChange={e => setTopic(e.target.value)}
						/>
					</div>
				</label>
				<label>
					Number of Respondents
					<div>
						<input
							type="number"
							value={numberOfRespondents}
							onChange={e => setNumberOfRespondents(Number(e.target.value))}
						/>
					</div>
				</label>

				<span>Options</span>

				{options.map(option => (
					<div key={option.id}>
						<label>
							#{option.id}
							<div>
								<input
									type="text"
									value={option.optionText}
									onChange={e => updateOption(option.id, e.target.value)}
								/>
							</div>
						</label>
					</div>
				))}

				<div>
					<button
						type="button"
						onClick={() =>
							setOptions([
								...options,
								{ id: options.length + 1, optionText: '' }
							])
						}
					>
						+ Add Option
					</button>
				</div>

				<button type="submit">Create Survey</button>
			</form>
		</div>
	);
};

function App() {
	const [surveyId, setSurveyId] = useState(1);
	const [surveyDetail, setSurveyDetail] = useState(null);

	const fetchSurvey = async surveyId => {
		const response = await fetch(`/api/survey/${surveyId}`);
		const data = await response.json();

		setSurveyDetail(data);
	};

	const createSurvey = async surveyCommand => {
		const response = await fetch(`/api/survey`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(surveyCommand)
		});
		const data = await response.json();

		setSurveyId(data.id);
		setSurveyDetail(data);
	};

	return (
		<div>
			<h1>Fake Survey Generator UI</h1>

			<p>This UI is basic AF</p>

			<p>It can just:</p>
			<ul>
				<li>Fetch a survey by ID</li>
				<li>Create a new survey</li>
			</ul>

			<p>That is all.</p>

			<h2>Get Survey</h2>
			<GetSurvey
				surveyId={surveyId}
				onUpdateSurveyId={value => setSurveyId(value)}
				onFetch={() => fetchSurvey(surveyId)}
				surveyDetail={surveyDetail}
			/>

			<h2>Create Survey</h2>
			<CreateSurvey onCreateSurvey={createSurvey} />
		</div>
	);
}

export default App;
